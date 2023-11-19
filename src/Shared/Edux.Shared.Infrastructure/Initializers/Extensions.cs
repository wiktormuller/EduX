using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Initializers
{
    internal static class Extensions
    {
        public static IServiceCollection AddInitializerToRegistry<TInitializer>(this IServiceCollection services) 
            where TInitializer : IInitializer
        {
            using var serviceProvider = services.BuildServiceProvider();

            var startupInitializer = serviceProvider.GetRequiredService<IStartupInitializer>();
            var initializer = serviceProvider.GetRequiredService<TInitializer>();
            startupInitializer.AddInitializer(initializer);

            return services;
        }
    }
}
