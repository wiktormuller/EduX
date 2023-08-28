using Edux.Shared.Abstractions.Crypto;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Commands;
using Edux.Shared.Infrastructure.Crypto;
using Edux.Shared.Infrastructure.Events;
using Edux.Shared.Infrastructure.Queries;
using Edux.Shared.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edux.Shared.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IList<Assembly> assemblies)
        {
            services.AddSingleton<IClock, UtcClock>();
            services.AddQueries(assemblies);
            services.AddCommands(assemblies);
            services.AddEvents(assemblies);
            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
