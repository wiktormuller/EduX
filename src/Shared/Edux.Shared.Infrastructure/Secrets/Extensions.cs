using Microsoft.AspNetCore.Builder;
using Edux.Shared.Infrastructure.Secrets.Vault;
using Microsoft.Extensions.DependencyInjection;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods.UserPass;
using VaultSharp;
using Edux.Shared.Abstractions.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Newtonsoft.Json;
using VaultSharp.V1.SecretsEngines;

namespace Edux.Shared.Infrastructure.Secrets
{
    internal static class Extensions
    {
        private const string SectionName = "vault";

        private static readonly ILeaseService LeaseService = new LeaseService();
        private static readonly ICertificatesService CertificatesService = new CertificatesService();

        public static WebApplicationBuilder InstallVault(this WebApplicationBuilder builder)
        {
            var options = builder.Services.GetOptions<VaultOptions>(SectionName);
            builder.Services.AddSingleton(options);

            VerifyOptions(options);

            builder.Services.AddVault(options);
            builder.Configuration.AddVaultAsync(options).GetAwaiter().GetResult();

            return builder;
        }

        private static IServiceCollection AddVault(this IServiceCollection services, VaultOptions options)
        {
            services.AddTransient<IKeyValueSecrets, KeyValueSecrets>();

            var (client, settings) = GetClientAndSettings(options);
            services.AddSingleton(settings);
            services.AddSingleton(client);

            services.AddSingleton(LeaseService);
            services.AddSingleton(CertificatesService);

            services.AddHostedService<VaultHostedService>();

            if (options.PKI.Enabled)
            {
                services.AddSingleton<ICertificatesIssuer, CertificatesIssuer>();
            }
            else
            {
                services.AddSingleton<ICertificatesIssuer, EmptyCertificatesIssuer>();
            }

            return services;
        }

        private static async Task AddVaultAsync(this IConfigurationBuilder builder, VaultOptions options)
        {
            var (client, _) = GetClientAndSettings(options);
            if (options.KV.Enabled)
            {
                var kvPath = options.KV.Path;
                var mountPoint = options.KV.MountPoint;
                if (string.IsNullOrWhiteSpace(kvPath))
                {
                    throw new ArgumentException("KV path is missing.");
                }

                if (string.IsNullOrWhiteSpace(mountPoint))
                {
                    throw new ArgumentException("KV mount point is missing.");
                }

                Console.WriteLine($"Loading settings from Vault: '{options.Url}', KV path: '{kvPath}'.");
                var keyValueSecrets = new KeyValueSecrets(client, options);
                var secret = await keyValueSecrets.GetAsync(kvPath);
                var parser = new JsonParser();
                var json = JsonConvert.SerializeObject(secret);
                var data = parser.Parse(json);
                var source = new MemoryConfigurationSource { InitialData = data };
                builder.Add(source);
            }

            if (options.PKI is not null && options.PKI.Enabled)
            {
                Console.WriteLine("Initializing Vault PKI.");
                await SetPkiSecretsAsync(client, options);
            }

            if (options.Lease is null || !options.Lease.Any())
            {
                return;
            }

            var configuration = new Dictionary<string, string>();
            foreach (var (key, lease) in options.Lease)
            {
                if (!lease.Enabled || string.IsNullOrWhiteSpace(lease.Type))
                {
                    continue;
                }

                Console.WriteLine($"Initializing Vault lease for: '{key}', type: '{lease.Type}'.");
                await InitLeaseAsync(key, client, lease, configuration);
            }

            if (configuration.Any())
            {
                var source = new MemoryConfigurationSource { InitialData = configuration };
                builder.Add(source);
            }
        }

        private static async Task SetPkiSecretsAsync(IVaultClient client, VaultOptions options)
        {
            var issuer = new CertificatesIssuer(client, options);
            var certificate = await issuer.IssueAsync();
            CertificatesService.Set(options.PKI.RoleName, certificate);
        }

        private static Task InitLeaseAsync(string key, IVaultClient client, VaultOptions.LeaseOptions options,
        IDictionary<string, string> configuration)
            => options.Type.ToLowerInvariant() switch
            {
                "database" => SetDatabaseSecretsAsync(key, client, options, configuration),
                "rabbitmq" => SetRabbitMqSecretsAsync(key, client, options, configuration),
                _ => Task.CompletedTask
            };

        private static async Task SetDatabaseSecretsAsync(string key, IVaultClient client,
            VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration)
        {
            const string name = SecretsEngineMountPoints.Defaults.Database;
            var mountPoint = string.IsNullOrWhiteSpace(options.MountPoint) ? name : options.MountPoint;
            var credentials =
                await client.V1.Secrets.Database.GetCredentialsAsync(options.RoleName, mountPoint);
            SetSecrets(key, options, configuration, name, () => (credentials, new Dictionary<string, string>
            {
                ["username"] = credentials.Data.Username,
                ["password"] = credentials.Data.Password
            }, credentials.LeaseId, credentials.LeaseDurationSeconds, credentials.Renewable));
        }

        private static async Task SetRabbitMqSecretsAsync(string key, IVaultClient client,
            VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration)
        {
            const string name = SecretsEngineMountPoints.Defaults.RabbitMQ;
            var mountPoint = string.IsNullOrWhiteSpace(options.MountPoint) ? name : options.MountPoint;
            var credentials =
                await client.V1.Secrets.RabbitMQ.GetCredentialsAsync(options.RoleName, mountPoint);
            SetSecrets(key, options, configuration, name, () => (credentials, new Dictionary<string, string>
            {
                ["username"] = credentials.Data.Username,
                ["password"] = credentials.Data.Password
            }, credentials.LeaseId, credentials.LeaseDurationSeconds, credentials.Renewable));
        }
        private static void SetSecrets(string key, VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration, string name,
            Func<(object, Dictionary<string, string>, string, int, bool)> lease)
        {
            var createdAt = DateTime.UtcNow;
            var (credentials, values, leaseId, duration, renewable) = lease();
            SetTemplates(key, options, configuration, values);
            var leaseData = new LeaseData(name, leaseId, duration, renewable, createdAt, credentials);
            LeaseService.Set(key, leaseData);
        }

        private static void SetTemplates(string key, VaultOptions.LeaseOptions lease,
            IDictionary<string, string> configuration, IDictionary<string, string> values)
        {
            if (lease.Templates is null || !lease.Templates.Any())
            {
                return;
            }

            foreach (var (property, template) in lease.Templates)
            {
                if (string.IsNullOrWhiteSpace(property) || string.IsNullOrWhiteSpace(template))
                {
                    continue;
                }

                var templateValue = $"{template}";
                templateValue = values.Aggregate(templateValue,
                    (current, value) => current.Replace($"{{{{{value.Key}}}}}", value.Value));
                configuration.Add($"{key}:{property}", templateValue);
            }
        }

        private static (IVaultClient client, VaultClientSettings settings) GetClientAndSettings(VaultOptions options)
        {
            var settings = new VaultClientSettings(options.Url, GetAuthMethod(options));
            var client = new VaultClient(settings);

            return (client, settings);
        }

        private static IAuthMethodInfo GetAuthMethod(VaultOptions options)
        {
            return options?.Authentication?.Type.ToLowerInvariant() switch
            {
                "token"
                    => new TokenAuthMethodInfo(options.Authentication.Token.Token),
                "userpass"
                    => new UserPassAuthMethodInfo(options.Authentication.UserPass.Username, options.Authentication.UserPass.Password),
                _ => throw new ArgumentException($"Vault auth type: '{options.Authentication.Type}' is not supported.")
            };
        }

        private static void VerifyOptions(VaultOptions options)
        {
            options.KV.EngineVersion = options.KV.EngineVersion switch
            {
                > 2 or < 0
                    => throw new ArgumentException($"Invalid KV Engine Version: {options.KV.EngineVersion} (available: 1 or 2)."),

                0 => 2,

                _ => options.KV.EngineVersion
            };
        }
    }
}
