using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Infrastructure.Logging.Decorators;
using Edux.Shared.Infrastructure.Logging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;
using FileOptions = Edux.Shared.Infrastructure.Logging.Options.FileOptions;

namespace Edux.Shared.Infrastructure.Logging
{
    internal static class Extensions
    {
        private const string ConsoleOutputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}";
        private const string FileOutputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";
        private const string AppSectionName = "app";
        private const string LoggerSectionName = "logger";

        public static IServiceCollection AddCommandHandlersLoggingDecorators(this IServiceCollection services)
        {
            services.TryDecorate(typeof(ICommandHandler<>), typeof(CommandHandlerLoggingDecorator<>));
            // TODO: Do the same for event handlers

            return services;
        }

        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .UseSerilog((context, loggerConfiguration) =>
                {
                    var loggerOptions = context.Configuration.GetOptions<LoggerOptions>(LoggerSectionName);
                    var appOptions = context.Configuration.GetOptions<AppOptions>(AppSectionName);

                    Configure(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                });
        }

        private static void Configure(LoggerOptions loggerOptions, AppOptions appOptions, 
            LoggerConfiguration loggerConfiguration, string environmentName)
        {
            var level = GetLogEventLevel(loggerOptions.Level);

            loggerConfiguration.Enrich.FromLogContext()
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

            var loggingLevelSwitch = new LoggingLevelSwitch();
            loggingLevelSwitch.MinimumLevel = GetLogEventLevel(options.Level);


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
                }).MinimumLevel.ControlledBy(loggingLevelSwitch);
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
                        period: lokiOptions.Period).MinimumLevel.ControlledBy(loggingLevelSwitch);
                }
                else
                {
                    loggerConfiguration.WriteTo.GrafanaLoki(
                        lokiOptions.Url,
                        batchPostingLimit: lokiOptions.BatchPostingLimit ?? 1000,
                        queueLimit: lokiOptions.QueueLimit,
                        period: lokiOptions.Period).MinimumLevel.ControlledBy(loggingLevelSwitch);
                }
            }

            if (seqOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
            }
        }

        private static LogEventLevel GetLogEventLevel(string level)
        {
            return Enum.TryParse<LogEventLevel>(level, true, out var logLevel)
                ? logLevel
                : LogEventLevel.Information;
        }
    }
}
