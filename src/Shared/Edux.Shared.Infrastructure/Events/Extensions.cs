using Edux.Shared.Abstractions.Events;
using Edux.Shared.Infrastructure.Decorator;
using Edux.Shared.Infrastructure.Events.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Events
{
    internal static class Extensions
    {
        public static IServiceCollection AddEvents(this IServiceCollection services, IList<Assembly> assemblies)
        {
            services.AddSingleton<IEventDispatcher, EventDispatcher>();

            services.Scan(scan =>
                scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(IEventHandler<>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
