using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Infrastructure.App.Options;
using Edux.Shared.Infrastructure.Observability.Logging.Decorators;
using Edux.Shared.Infrastructure.Observability.Logging.Options;
using Edux.Shared.Infrastructure.Observability.Logging.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;
using FileOptions = Edux.Shared.Infrastructure.Observability.Logging.Options.FileOptions;

namespace Edux.Shared.Infrastructure.Observability.Logging
{
    internal static class Extensions
    {
        private const string ConsoleOutputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}";
        private const string FileOutputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";
        private const string AppSectionName = "app";
        private const string LoggerSectionName = "logger";

        internal static LoggingLevelSwitch LoggingLevelSwitch = new();

        public static IServiceCollection AddCommandHandlersLoggingDecorators(this IServiceCollection services)
        {
            services.TryDecorate(typeof(ICommandHandler<>), typeof(CommandHandlerLoggingDecorator<>));
            services.TryDecorate(typeof(IDomainEventHandler<>), typeof(DomainEventHandlerLoggingDecorator<>));

            return services;
        }

        public static IHostBuilder InstallLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(services => services.AddSingleton<ILoggingService, LoggingService>())
                .UseSerilog((context, loggerConfiguration) =>
                {
                    var loggerOptions = context.Configuration.GetOptions<LoggerOptions>(LoggerSectionName);
                    var appOptions = context.Configuration.GetOptions<AppOptions>(AppSectionName);

                    Configure(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                });
        }

        public static IApplicationBuilder UseCorrelationContextLogging(this IApplicationBuilder app)
        {
            app.Use(async (httpContext, next) =>
            {
                var logger = httpContext.RequestServices.GetRequiredService<ILogger<IContextAccessor>>();
                var context = httpContext.RequestServices.GetRequiredService<IContextProvider>().Current();

                var userId = context.IdentityContext.IsAuthenticated
                    ? context.IdentityContext.Id.ToString("N")
                    : string.Empty;

                logger.LogInformation($"Started processing a request " +
                    $"[Request ID: '{context.RequestContext.RequestId}', Correlation ID: '{context.CorrelationId}', " +
                    $"Trace ID: '{context.TraceId}', User ID: '{userId}']...");

                await next();

                logger.LogInformation($"Finished processing a request with status code: {httpContext.Response.StatusCode} " +
                    $"[Request ID: '{context.RequestContext.RequestId}', Correlation ID: '{context.CorrelationId}', " +
                    $"Trace ID: '{context.TraceId}', User ID: '{userId}']");
            });

            return app;
        }

        public static IEndpointConventionBuilder MapLogLevelEndpoint(this IEndpointRouteBuilder builder,
            string endpointRoute = "~/logging/level")
        {
            return builder.MapPost(endpointRoute, SwitchLoggingLevel);
        }

        public static LogEventLevel GetLogEventLevel(string level)
        {
            return Enum.TryParse<LogEventLevel>(level, true, out var logLevel)
                ? logLevel
                : LogEventLevel.Information;
        }

        private static void Configure(LoggerOptions loggerOptions, AppOptions appOptions,
            LoggerConfiguration loggerConfiguration, string environmentName)
        {
            LoggingLevelSwitch.MinimumLevel = GetLogEventLevel(loggerOptions.Level);

            loggerConfiguration.Enrich.FromLogContext()
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.WithProperty("Application", appOptions.Name)
                .Enrich.WithProperty("Instance", appOptions.Instance)
                .Enrich.WithProperty("Version", appOptions.Version);

            foreach (var (key, value) in loggerOptions.Tags)
            {
                loggerConfiguration.Enrich.WithProperty(key, value);
            }

            foreach (var (key, value) in loggerOptions.MinimumLevelOverrides)
            {
                var logLevel = GetLogEventLevel(value);
                loggerConfiguration.MinimumLevel.Override(key, logLevel);
            }

            loggerOptions.ExcludePaths?.ToList().ForEach(p => loggerConfiguration.Filter
                .ByExcluding(Matching.WithProperty<string>("RequestPath", n => n.EndsWith(p))));

            loggerOptions.ExcludeProperties?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty(p)));

            Configure(loggerConfiguration, loggerOptions);
        }

        private static void Configure(LoggerConfiguration loggerConfiguration, LoggerOptions options)
        {
            var consoleOptions = options.Console ?? new ConsoleOptions();
            var fileOptions = options.File ?? new FileOptions();
            var elkOptions = options.Elk ?? new ElkOptions();
            var seqOptions = options.Seq ?? new SeqOptions();
            var lokiOptions = options.Loki ?? new LokiOptions();

            if (consoleOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Console(outputTemplate: ConsoleOutputTemplate);
            }

            if (fileOptions.Enabled)
            {
                var path = string.IsNullOrWhiteSpace(fileOptions.Path) ? "logs/logs.txt" : fileOptions.Path;
                if (!Enum.TryParse<RollingInterval>(fileOptions.Interval, true, out var interval))
                {
                    interval = RollingInterval.Day;
                }

                loggerConfiguration.WriteTo.File(path, rollingInterval: interval, outputTemplate: FileOutputTemplate);
            }

            if (elkOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elkOptions.Url))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = string.IsNullOrWhiteSpace(elkOptions.IndexFormat)
                        ? "logstash-{0:yyyy.MM.dd}"
                        : elkOptions.IndexFormat,
                    ModifyConnectionSettings = connectionConfiguration =>
                        elkOptions.BasicAuthEnabled
                            ? connectionConfiguration.BasicAuthentication(elkOptions.Username, elkOptions.Password)
                            : connectionConfiguration
                }).MinimumLevel.ControlledBy(LoggingLevelSwitch);
            }

            if (lokiOptions.Enabled)
            {
                if (lokiOptions.LokiUsername is not null && lokiOptions.LokiPassword is not null)
                {
                    var auth = new LokiCredentials
                    {
                        Login = lokiOptions.LokiUsername,
                        Password = lokiOptions.LokiPassword
                    };

                    loggerConfiguration.WriteTo.GrafanaLoki(
                        lokiOptions.Url,
                        credentials: auth,
                        batchPostingLimit: lokiOptions.BatchPostingLimit ?? 1000,
                        queueLimit: lokiOptions.QueueLimit,
                        period: lokiOptions.Period).MinimumLevel.ControlledBy(LoggingLevelSwitch);
                }
                else
                {
                    loggerConfiguration.WriteTo.GrafanaLoki(
                        lokiOptions.Url,
                        batchPostingLimit: lokiOptions.BatchPostingLimit ?? 1000,
                        queueLimit: lokiOptions.QueueLimit,
                        period: lokiOptions.Period).MinimumLevel.ControlledBy(LoggingLevelSwitch);
                }
            }

            if (seqOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
            }
        }

        private static async Task SwitchLoggingLevel(HttpContext context)
        {
            var service = context.RequestServices.GetService<ILoggingService>();
            if (service is null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("ILoggingService is not registered. Add UseLogging() to your Program.cs.");
                return;
            }

            var level = context.Request.Query["level"].ToString();

            if (string.IsNullOrWhiteSpace(level))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid value for logging level.");
                return;
            }

            service.SetLoggingLevel(level);

            context.Response.StatusCode = StatusCodes.Status200OK;
        }
    }
}
