using Microsoft.AspNetCore.Builder;
using Edux.Shared.Infrastructure.Secrets.Vault;
using Microsoft.Extensions.DependencyInjection;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods.UserPass;
using VaultSharp;
using Edux.Shared.Abstractions.Secrets;
using Microsoft.Extensions.Configuration;

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

        private static async Task AddVaultAsync(this IConfigurationBuilder builder, VaultOptions options, string keyValuePath)
        {
            // TODO: Implement
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
