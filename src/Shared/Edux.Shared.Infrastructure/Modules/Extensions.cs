using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Modules;
using Edux.Shared.Infrastructure.Modules.Clients;
using Edux.Shared.Infrastructure.Modules.Info;
using Edux.Shared.Infrastructure.Modules.Registries;
using Edux.Shared.Infrastructure.Modules.Serializers;
using Edux.Shared.Infrastructure.Modules.Subscribers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Modules
{
    public static class Extensions
    {
        public static IModuleSubscriber UseModuleRequests(this IApplicationBuilder app)
            => app.ApplicationServices.GetRequiredService<IModuleSubscriber>();

        internal static IHostBuilder LoadModuleSettings(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureAppConfiguration((context, config) =>
            {
                foreach (var settings in context.GetSettings("*"))
                {
                    config.AddJsonFile(settings);
                }

                foreach (var settings in context.GetSettings($"*.{context.HostingEnvironment.EnvironmentName}"))
                {
                    config.AddJsonFile(settings);
                }
            });
        }

        internal static IServiceCollection AddModuleRequests(this IServiceCollection services, IList<Assembly> assemblies)
        {
            services.AddModuleRegistry(assemblies);
            services.AddSingleton<IModuleClient, ModuleClient>();
            services.AddSingleton<IModuleSubscriber, ModuleSubscriber>();
            services.AddSingleton<IModuleSerializer, JsonModuleSerializer>();

            return services;
        }

        internal static IEnumerable<string> GetSettings(this HostBuilderContext context, string pattern)
        {
            return Directory.EnumerateFiles(
                context.HostingEnvironment.ContentRootPath, 
                $"module.{pattern}.json", SearchOption.AllDirectories);
        }

        internal static IServiceCollection AddModuleInfo(this IServiceCollection services, IList<IModule> modules)
        {
            var moduleInfoProvider = new ModuleInfoProvider();
            var moduleInfo = modules
                ?.Select(m => new ModuleInfo(m.Name, m.Path, m.Policies ?? Enumerable.Empty<string>())) ??
                    Enumerable.Empty<ModuleInfo>();

            moduleInfoProvider.Modules.AddRange(moduleInfo);

            services.AddSingleton(moduleInfoProvider);

            return services;
        }

        internal static IEnumerable<string> GetDisabledModules(IServiceCollection services)
        {
            var disabledModules = new List<string>();
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                foreach (var (key, value) in configuration.AsEnumerable())
                {
                    if (!key.Contains(":module:enabled"))
                    {
                        continue;
                    }

                    if (!bool.Parse(value))
                    {
                        disabledModules.Add(key.Split(":")[0]);
                    }
                }
            }

            return disabledModules;
        }

        internal static void MapModuleInfo(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet("modules", context =>
            {
                var moduleInfoProvider = context.RequestServices.GetRequiredService<ModuleInfoProvider>();
                return context.Response.WriteAsJsonAsync(moduleInfoProvider.Modules);
            });
        }

        private static void AddModuleRegistry(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var registry = new ModuleRegistry();
            var types = assemblies
                .SelectMany(a => a.GetTypes())
                .ToArray();

            var eventTypes = types // If we want to process integration events synchronously (not possible when migrate some module to microservices)
                .Where(t => t.IsClass && typeof(IEvent).IsAssignableFrom(t))
                .ToArray();

            services.AddSingleton<IModuleRegistry>(sp =>
            {
                var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                var eventDispatcherType = eventDispatcher.GetType();

                foreach (var type in eventTypes)
                {
                    registry.AddBroadcastAction(type, (@event) =>
                        (Task)eventDispatcherType.GetMethod(nameof(eventDispatcher.PublishAsync))
                    ?.MakeGenericMethod(type)
                            .Invoke(eventDispatcher, new[] { @event }));
                }

                return registry;
            });
        }
    }
}
