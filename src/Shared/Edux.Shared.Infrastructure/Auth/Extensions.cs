using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Modules;
using Edux.Shared.Infrastructure.Auth.Options;
using Edux.Shared.Infrastructure.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Edux.Shared.Infrastructure.Auth
{
    internal static class Extensions
    {
        public static IServiceCollection AddAuth(this IServiceCollection services, IList<IModule> modules)
        {
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<IJwtProvider, JwtProvider>();
            services.AddSingleton<ITokenStorage, HttpContextTokenStorage>();
            services.AddSingleton<IPasswordHasher<PasswordService>, PasswordHasher<PasswordService>>();

            var options = services.GetOptions<AuthOptions>("auth");
            services.AddSingleton(options);

            if (options.AuthenticationDisabled)
            {
                services.AddSingleton<IPolicyEvaluator, DisabledAuthenticationPolicyEvaluator>(); // TODO : Test it
            }

            var tokenValidationParameters = BuildTokenValidationParameters(options);
            services.AddSingleton(tokenValidationParameters);

            services
                .AddAuthentication(authOptions =>
                {
                    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(authOptions =>
                {
                    authOptions.Authority = options.Authority;
                    authOptions.Audience = options.Audience;
                    authOptions.MetadataAddress = options.MetadataAddress;
                    authOptions.SaveToken = options.SaveToken;
                    authOptions.RefreshOnIssuerKeyNotFound = options.RefreshOnIssuerKeyNotFound;
                    authOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    authOptions.IncludeErrorDetails = options.IncludeErrorDetails;
                    authOptions.TokenValidationParameters = tokenValidationParameters;
                    if (!string.IsNullOrWhiteSpace(options.Challenge))
                    {
                        authOptions.Challenge = options.Challenge;
                    }
                });

            var policies = modules?.SelectMany(x => x.Policies ?? Enumerable.Empty<string>()) ??
                Enumerable.Empty<string>();

            services.AddAuthorization(authOptions =>
            {
                foreach (var policy in policies) // Register existing policies in system
                {
                    authOptions.AddPolicy(policy, x => x.RequireClaim("permissions", policy));
                }

                authOptions.AddPolicy("is-admin", x => x.RequireRole("admin"));
            });

            return services;
        }

        private static TokenValidationParameters BuildTokenValidationParameters(AuthOptions options)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                RequireAudience = options.RequireAudience,
                ValidIssuer = options.ValidIssuer,
                ValidIssuers = options.ValidIssuers,
                ValidateActor = options.ValidateActor,
                ValidAudience = options.ValidAudience,
                ValidAudiences = options.ValidAudiences,
                ValidateAudience = options.ValidateAudience,
                ValidateIssuer = options.ValidateIssuer,
                ValidateLifetime = options.ValidateLifetime,
                ValidateTokenReplay = options.ValidateTokenReplay,
                ValidateIssuerSigningKey = options.ValidateIssuerSigningKey,
                SaveSigninToken = options.SaveSigninToken,
                RequireExpirationTime = options.RequireExpirationTime,
                RequireSignedTokens = options.RequireSignedTokens,
                ClockSkew = TimeSpan.Zero
            };

            if (string.IsNullOrWhiteSpace(options.IssuerSigningKey))
            {
                throw new ArgumentException("Missin issuer signing key.", nameof(options.IssuerSigningKey));
            }

            if (!string.IsNullOrWhiteSpace(options.AuthenticationType))
            {
                tokenValidationParameters.AuthenticationType = options.AuthenticationType;
            }

            var rawKey = Encoding.UTF8.GetBytes(options.IssuerSigningKey);
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(rawKey);

            if (!string.IsNullOrWhiteSpace(options.NameClaimType))
            {
                tokenValidationParameters.NameClaimType = options.NameClaimType;
            }

            if (!string.IsNullOrWhiteSpace(options.RoleClaimType))
            {
                tokenValidationParameters.RoleClaimType = options.RoleClaimType;
            }

            return tokenValidationParameters;
        }
    }
}
