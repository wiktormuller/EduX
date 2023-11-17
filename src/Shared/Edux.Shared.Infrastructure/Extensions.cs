using Edux.Shared.Abstractions.Crypto;
using Edux.Shared.Abstractions.Modules;
using Edux.Shared.Abstractions.Serializers;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Api;
using Edux.Shared.Infrastructure.Auth;
using Edux.Shared.Infrastructure.Commands;
using Edux.Shared.Infrastructure.Crypto;
using Edux.Shared.Infrastructure.Events;
using Edux.Shared.Infrastructure.Exceptions;
using Edux.Shared.Infrastructure.Features;
using Edux.Shared.Infrastructure.SharedKernel;
using Edux.Shared.Infrastructure.Messaging;
using Edux.Shared.Infrastructure.Modules;
using Edux.Shared.Infrastructure.Observability;
using Edux.Shared.Infrastructure.Queries;
using Edux.Shared.Infrastructure.Serializers;
using Edux.Shared.Infrastructure.Storage;
using Edux.Shared.Infrastructure.Time;
using Edux.Shared.Infrastructure.Transactions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Initializers;
using Edux.Shared.Infrastructure.Observability.Logging;
using Edux.Shared.Infrastructure.Storage.SqlServer;
using Edux.Shared.Infrastructure.Storage.SqlServer.Initializers;
using Edux.Shared.Infrastructure.Contexts;
using Edux.Shared.Infrastructure.WebSockets;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using GraphQL;
using Edux.Shared.Infrastructure.Api.Graphql;
using Microsoft.AspNetCore.WebSockets;
using Edux.Shared.Abstractions.Api.Graphql;
using Edux.Shared.Infrastructure.Api.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Edux.Shared.Infrastructure.Storage.Redis;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

[assembly: InternalsVisibleTo("Edux.Bootstrapper")]
[assembly: InternalsVisibleTo("Edux.Shared.Tests")]
namespace Edux.Shared.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
            IList<Assembly> assemblies, IList<IModule> modules)
        {
            services.AddErrorHandling();

            services.AddAuth(modules);
            services.AddSingleton<IClock, UtcClock>();
            services.AddQueries(assemblies);
            services.AddCommands(assemblies);
            services.AddEvents(assemblies);
            services.AddDomainEvents(assemblies);
            services.AddMessaging();
            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddHttpContextAccessor();
            services.AddContext();
            services.AddRabbitMq();

            services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();

            services.AddHostedService<DbAppInitializer>();

            services.AddTransactionalDecorators();
            services.AddCommandHandlersLoggingDecorators();

            services.AddOutbox();
            services.AddInbox();
            services.AddMsSqlServer();

            services.AddRedis();
            services.AddFeatureFlags();
            services.AddObservability(assemblies);

            services.AddModuleInfo(modules);
            services.AddModuleRequests(assemblies);
            services.AddControllersWithOpenApi();

            services.AddSignalr();
            services.AddGrpc();

            services.AddSingleton<EduxCompositeQuery>();
            services.AddSingleton<EduxCompositeSubscription>();
            services
                .AddGraphQL(config =>
                {
                    config.AddSchema<EduxSchema>()
                        .AddSystemTextJson()
                        .AddUserContextBuilder(httpContext => new GraphContext(httpContext))
                        .AddErrorInfoProvider((options, serviceProvider) =>
                        {
                            options.ExposeExceptionStackTrace = false; // TODO: It should comes from settings
                        });
                })
                .AddWebSockets(config =>
                {
                    config.KeepAliveInterval = TimeSpan.FromSeconds(5);
                });

            services.AddSingleton<DbAppInitializerHealthCheck>();
            services.AddHealthChecks()
                .AddCheck<SampleHealthCheck>("sample-check", tags: new[] { "live" })
                .AddCheck<DbAppInitializerHealthCheck>("DbAppInitializer", tags: new[] { "ready" })
                .AddRabbitMQ(rabbitConnectionString: services.GetOptions<RabbitMqOptions>("rabbitmq").GetConnectionString(), tags: new[] { "live" })
                .AddRedis(redisConnectionString: services.GetOptions<RedisOptions>("redis").ConnectionString, tags: new[] { "live" });

            services.AddRequestTimeouts(options =>
            {
                options.DefaultPolicy = new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromMilliseconds(3000),
                    TimeoutStatusCode = 503
                };
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseResponseCompression();
            app.UseContext();
            app.UseInitializers();
            app.UseHttpsRedirection();
            app.UseCors("cors");
            app.UseObservability();
            app.UseErrorHandling();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseReDoc(reDoc =>
            {
                reDoc.RoutePrefix = "docs";
                reDoc.SpecUrl("/swagger/v1/swagger.json");
                reDoc.DocumentTitle = "Edux API";
            });
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseRequestTimeouts();
            app.UseRateLimiter();

            app.UseEndpoints(endpointRouteBuilder =>
            {
                endpointRouteBuilder.MapControllers();
                endpointRouteBuilder.MapGet("/", () => "Edux API!");
                endpointRouteBuilder.MapGet("/grpc-endpoints", async context =>
                {
                    var endpointDataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();

                    await context.Response.WriteAsJsonAsync(new
                    {
                        Results = endpointDataSource
                            .Endpoints
                            .OfType<RouteEndpoint>()
                            .Where(e => e.DisplayName?.StartsWith("gRPC - /") is true)
                            .Select(e => new
                            {
                                Name = e.DisplayName,
                                Pattern = e.RoutePattern.RawText,
                                Order = e.Order
                            })
                            .ToList()
                    });
                });
                endpointRouteBuilder.MapModuleInfo();
                endpointRouteBuilder.MapLogLevelEndpoint("~/logging/level");
            });

            app.UseWebSockets();
            app.UseGraphQL<EduxSchema>(path: "/graphql");

            app.UseGraphQLPlayground("/ui/playground",
            new GraphQL.Server.Ui.Playground.PlaygroundOptions
            {
                GraphQLEndPoint = "/graphql",
                SubscriptionsEndPoint = "/graphql",
            });

            ((WebApplication)app).MapHealthChecks("/health-checks/live", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("live"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            ((WebApplication)app).MapHealthChecks("/health-checks/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }

        public static IApplicationBuilder ShareProtoFiles(this IApplicationBuilder app, 
            string requestPath, string protoFilePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Clear();
            provider.Mappings[".proto"] = "text/plain";

            return app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(protoFilePath),
                RequestPath = requestPath,
                ContentTypeProvider = provider
            });
        }

        public static IApplicationBuilder UseInitializers(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var startupInitializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
            Task.Run(() => startupInitializer.InitializeAsync()).GetAwaiter().GetResult();

            return app;
        }

        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            return configuration.GetOptions<T>(sectionName);
        }

        public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
        {
            var options = new T();
            configuration.GetSection(sectionName).Bind(options);
            return options;
        }

        public static T BindOptions<T>(this IConfigurationSection section) where T : new()
        {
            var options = new T();
            section.Bind(options);
            return options;
        }

        public static string GetModuleName(this object value)
            => value?.GetType().GetModuleName() ?? string.Empty;

        public static string GetModuleName(this Type type, string namespacePart = "Modules", int splitIndex = 2)
        {
            if (type?.Namespace is null)
            {
                return string.Empty;
            }

            return type.Namespace.Contains(namespacePart)
                ? type.Namespace.Split(".")[splitIndex].ToLowerInvariant()
                : string.Empty;
        }
    }
}
