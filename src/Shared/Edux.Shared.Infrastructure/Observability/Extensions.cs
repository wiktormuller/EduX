using Edux.Shared.Infrastructure.Observability.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Observability
{
    internal static class Extensions
    {
        public static IServiceCollection AddObservability(this IServiceCollection services, IList<Assembly> assemblies)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services.AddOpenTelemetry()
                .AddMetrics(services, configuration, assemblies);
                //.AddTracing(services, configuration);

            services.AddMessagingMetricsDecorators();

            return services;
        }

        public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
        {
            return app.UseMetrics();
        }
    }
}
