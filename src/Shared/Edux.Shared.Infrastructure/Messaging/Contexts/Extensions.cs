using Edux.Shared.Abstractions.Messaging.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Messaging.Contexts
{
    internal static class Extensions
    {
        public static IServiceCollection AddMessageContext(this IServiceCollection services)
        {
            services.AddSingleton<IMessageContextProvider, MessageContextProvider>();
            services.AddSingleton<IMessageContextAccessor, MessageContextAccessor>();

            return services;
        }
    }
}
