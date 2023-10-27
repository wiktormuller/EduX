using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Infrastructure.Decorator;
using Edux.Shared.Infrastructure.SharedKernel.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edux.Shared.Infrastructure.SharedKernel
{
    internal static class Extensions
    {
        public static IServiceCollection AddDomainEvents(this IServiceCollection services, IList<Assembly> assemblies)
        {
            services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

            services.Scan(scan =>
                scan.FromAssemblies(assemblies)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(IDomainEventHandler<>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
