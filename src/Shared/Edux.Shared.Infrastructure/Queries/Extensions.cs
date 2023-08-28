using Edux.Shared.Abstractions.Queries;
using Edux.Shared.Infrastructure.Decorator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Queries
{
    public static class Extensions
    {
        public static IServiceCollection AddQueries(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

            services.Scan(scan => 
                scan
                    .FromAssemblies(assemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>))
                        .WithoutAttribute<DecoratorAttribute>())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            return services;
        }
    }
}
