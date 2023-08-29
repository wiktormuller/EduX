using Edux.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Edux.Shared.Infrastructure.Modules
{
    public static class Extensions
    {
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

        private static IEnumerable<string> GetSettings(this HostBuilderContext context, string pattern)
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

        internal static void MapModuleInfo(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet("modules", context =>
            {
                var moduleInfoProvider = context.RequestServices.GetRequiredService<ModuleInfoProvider>();
                return context.Response.WriteAsJsonAsync(moduleInfoProvider.Modules);
            });
        }
    }
}
