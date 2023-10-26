using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Infrastructure.Messaging.Brokers;
using Edux.Shared.Infrastructure.Messaging.Outbox;
using Edux.Shared.Infrastructure.Messaging.Outbox.EF;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using Edux.Shared.Infrastructure.Messaging.Outbox.Processors;
using Edux.Shared.Infrastructure.Messaging.Outbox.Registries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Messaging
{
    public static class Extensions
    {
        internal static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddScoped<IMessageBroker, MessageBroker>();

            return services;
        }

        public static IServiceCollection AddOutbox<T>(this IServiceCollection services) where T : DbContext
        {
            var outboxOptions = services.GetOptions<OutboxOptions>("outbox");

            if (!outboxOptions.Enabled)
            {
                return services;
            }

            services.AddTransient<IMessageOutbox, EfMessageOutbox<T>>();
            services.AddTransient<EfMessageOutbox<T>>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<OutboxTypeRegistry>().Register<EfMessageOutbox<T>>();

            return services;
        }

        public static IServiceCollection AddOutbox(this IServiceCollection services)
        {
            var outboxOptions = services.GetOptions<OutboxOptions>("outbox");
            services.AddSingleton(outboxOptions);

            services.AddSingleton(new OutboxTypeRegistry());

            if (!outboxOptions.Enabled)
            {
                return services;
            }

            services.AddHostedService<OutboxMessageProcessor>();
            services.AddHostedService<OutboxMessageCleanupProcessor>();

            return services;
        }
    }
}
