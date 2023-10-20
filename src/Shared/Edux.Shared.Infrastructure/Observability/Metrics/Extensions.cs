using Edux.Shared.Abstraction.Observability.Metrics;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Infrastructure.Observability.Metrics.Decorators;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Observability.Metrics
{
    internal static class Extensions
    {
        public static OpenTelemetryBuilder AddMetrics(this OpenTelemetryBuilder openTelemetry, IServiceCollection services, 
            IConfiguration configuration,
            IList<Assembly> assemblies)
        {
            var section = configuration.GetSection("metrics");
            var options = section.BindOptions<MetricsOptions>();
            services.Configure<MetricsOptions>(section);

            if (!options.Enabled)
            {
                return openTelemetry;
            }

            return openTelemetry
                .WithMetrics(builder =>
                {
                    builder.AddAspNetCoreInstrumentation();
                    builder.AddHttpClientInstrumentation();
                    builder.AddRuntimeInstrumentation();
                    builder.AddProcessInstrumentation();

                    foreach (var attribute in GetMeterAttributes(assemblies))
                    {
                        if (attribute is not null)
                        {
                            builder.AddMeter(attribute.Key);
                        }
                    }

                    builder
                        .AddMeter("Microsoft.AspNetCore.Hosting")
                        .AddMeter("Microsoft.AspNetCore.Server.Kestrel");

                    switch (options.Exporter.ToLowerInvariant())
                    {
                        case "console":
                            builder.AddConsoleExporter();
                            break;

                        case "prometheus":
                            builder.AddPrometheusExporter(prometheus =>
                            {
                                prometheus.ScrapeEndpointPath = options.Endpoint;
                            });
                            break;
                    }
                });
        }

        public static IServiceCollection AddMessagingMetricsDecorators(this IServiceCollection services)
        {
            services.TryDecorate<IMessageBroker, MessageBrokerMetricsDecorator>();

            return services;
        }

        public static IApplicationBuilder UseMetrics(this IApplicationBuilder app)
        {
            var metricsOptions = app.ApplicationServices.GetRequiredService<IOptions<MetricsOptions>>().Value;

            if (!metricsOptions.Enabled)
            {
                return app;
            }

            if (metricsOptions.Exporter.ToLowerInvariant() != "prometheus")
            {
                return app;
            }

            return app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }

        private static IEnumerable<MeterAttribute?> GetMeterAttributes(IList<Assembly> assemblies)
        {
            return assemblies
                .Where(x => !x.IsDynamic)
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass && x.GetCustomAttribute<MeterAttribute>() is not null)
                .Select(x => x.GetCustomAttribute<MeterAttribute>())
                .Where(x => x is not null);
        }
    }
}
