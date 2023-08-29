using Microsoft.Extensions.Configuration;
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
    }
}
