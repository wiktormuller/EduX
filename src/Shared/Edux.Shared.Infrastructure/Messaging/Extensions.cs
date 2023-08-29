using Edux.Shared.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Messaging
{
    internal static class Extensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBroker, MessageBroker>();

            return services;
        }
    }
}
