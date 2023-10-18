using Edux.Shared.Infrastructure.Features.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace Edux.Shared.Infrastructure.Features
{
    internal static class Extensions
    {
        private const string SectionName = "azureAppConfigOptions";

        public static IServiceCollection AddFeatureFlags(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services
                .AddFeatureManagement(configuration.GetSection("featureManagement")) // IFeatureManager is available
                .AddFeatureFilter<PercentageFilter>()
                .AddFeatureFilter<TimeWindowFilter>()
                .AddFeatureFilter<TargetingFilter>();

            return services;
        }

        public static WebApplicationBuilder InstallAzureAppConfig(this WebApplicationBuilder builder)
        {
            var options = builder.Services.GetOptions<AzureAppConfigOptions>(SectionName);
            builder.Services.AddSingleton(options);

            builder.Configuration
                .AddAzureAppConfiguration(aaco => aaco.Connect(options.ConnectionString)
                    .UseFeatureFlags(ffo =>
                    {
                        ffo.CacheExpirationInterval = TimeSpan.FromMinutes(options.CacheExpirationInterval);
                        // ffo.Select("TestEnvironment:*", "dev"); // Filtering feature flags
                    }));

            builder.Services.AddAzureAppConfiguration();

            return builder;
        }

        public static IApplicationBuilder UseAzureAppConfiguration(this IApplicationBuilder app)
        {
            app.UseAzureAppConfiguration();

            return app;
        }
    }
}
