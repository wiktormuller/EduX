using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Security.Cryptography.X509Certificates;

namespace Edux.Shared.Infrastructure.HTTP
{
    public static class Extensions
    {
        public static IServiceCollection AddHttpClient(this IServiceCollection services, IConfiguration configuration,
            string sectionName)
        {
            var options = configuration.GetOptions<HttpClientOptions>(sectionName);
            services.AddSingleton(options);

            services.AddTransient<CorrelationIdMessageHandler>();

            // Typical Retry Policy to defend against network problems or temporary problems with server 
            var builder = services
                .AddHttpClient(options.Name)
                .AddHttpMessageHandler<CorrelationIdMessageHandler>()
                .AddHttpMessageHandler<LoggingHttpMessageHandler>()
                .AddTransientHttpErrorPolicy(_ => HttpPolicyExtensions.HandleTransientHttpError()
                    .WaitAndRetryAsync(options.Resiliency.Retries, retry =>
                        options.Resiliency.Exponential
                            ? TimeSpan.FromSeconds(Math.Pow(2, retry))
                            : options.Resiliency.RetryInterval ?? TimeSpan.FromSeconds(2)));

            // Certificate
            var certificateLocation = options.Certificate?.Location;
            if (options.Certificate is not null && !string.IsNullOrWhiteSpace(certificateLocation))
            {
                var certificate = new X509Certificate2(certificateLocation, options.Certificate.Password);
                builder.ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ClientCertificates.Add(certificate);
                    return handler;
                });
            }

            return services;
        }
    }
}
