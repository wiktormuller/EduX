using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Infrastructure.Commands.Dispatchers;
using Edux.Shared.Infrastructure.Decorator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Commands
{
    internal static class Extensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services, IList<Assembly> assemblies)
        {
            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

            services.Scan(scan => 
                scan.FromAssemblies(assemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>))
                        .WithoutAttribute<DecoratorAttribute>())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            return services;
        }
    }
}
