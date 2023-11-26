using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Outbox;
using Edux.Shared.Abstractions.SharedKernel.Types;
using Edux.Shared.Infrastructure.Initializers;
using Edux.Shared.Infrastructure.Messaging.Brokers;
using Edux.Shared.Infrastructure.Messaging.Inbox;
using Edux.Shared.Infrastructure.Messaging.Inbox.Decorators;
using Edux.Shared.Infrastructure.Messaging.Inbox.EF;
using Edux.Shared.Infrastructure.Messaging.Inbox.Mongo;
using Edux.Shared.Infrastructure.Messaging.Inbox.Options;
using Edux.Shared.Infrastructure.Messaging.Inbox.Processors;
using Edux.Shared.Infrastructure.Messaging.Inbox.Registries;
using Edux.Shared.Infrastructure.Messaging.Outbox;
using Edux.Shared.Infrastructure.Messaging.Outbox.EF;
using Edux.Shared.Infrastructure.Messaging.Outbox.Mongo;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using Edux.Shared.Infrastructure.Messaging.Outbox.Processors;
using Edux.Shared.Infrastructure.Messaging.Outbox.Registries;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Initializers;
using Edux.Shared.Infrastructure.Storage.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Edux.Shared.Infrastructure.Messaging
{
    public static class Extensions
    {
        internal static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddScoped<IMessageBroker, MessageBroker>();

            return services;
        }

        public static IServiceCollection AddOutboxCore(this IServiceCollection services)
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

        public static IServiceCollection AddEfOutbox<T>(this IServiceCollection services) where T : DbContext
        {
            var outboxOptions = services.GetOptions<OutboxOptions>("outbox");

            if (!outboxOptions.Enabled)
            {
                return services;
            }

            services.AddTransient<IMessageOutbox, EfMessageOutbox<T>>();
            services.AddTransient<EfMessageOutbox<T>>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<OutboxTypeRegistry>()
                .Register<EfMessageOutbox<T>>();

            return services;
        }

        public static IServiceCollection AddMongoOutbox<T>(this IServiceCollection services) where T : class
        {
            var outboxOptions = services.GetOptions<OutboxOptions>("outbox");

            if (!outboxOptions.Enabled)
            {
                return services;
            }

            services.AddTransient<IMessageOutbox, MongoMessageOutbox<T>>();
            services.AddTransient<MongoMessageOutbox<T>>();

            services.AddMongoRepository<OutboxMessage, string>("outbox");
            
            services.AddTransient<MongoOutboxInitializer>();
            services.AddInitializerToRegistry<RabbitMqExchangeInitializer>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<OutboxTypeRegistry>()
                .Register<MongoMessageOutbox<T>>();

            BsonClassMap.RegisterClassMap<OutboxMessage>(message =>
            {
                message.AutoMap();
            });

            return services;
        }

        public static IServiceCollection AddInboxCore(this IServiceCollection services)
        {
            var inboxOptions = services.GetOptions<InboxOptions>("inbox");
            services.AddSingleton(inboxOptions);

            services.AddSingleton(new InboxTypeRegistry());

            if (!inboxOptions.Enabled)
            {
                return services;
            }

            services.AddHostedService<InboxMessageCleanupProcessor>();

            services.TryDecorate(typeof(ICommandHandler<>), typeof(InboxCommandHandlerDecorator<>));
            services.TryDecorate(typeof(IEventHandler<>), typeof(InboxEventHandlerDecorator<>));

            return services;
        }

        public static IServiceCollection AddEfInbox<T>(this IServiceCollection services) where T : DbContext
        {
            var inboxOptions = services.GetOptions<InboxOptions>("inbox");

            if (!inboxOptions.Enabled)
            {
                return services;
            }

            services.AddTransient<IMessageInbox, EfMessageInbox<T>>();
            services.AddTransient<EfMessageInbox<T>>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<InboxTypeRegistry>()
                .Register<EfMessageInbox<T>>();

            return services;
        }

        public static IServiceCollection AddMongoInbox(this IServiceCollection services)
        {
            var inboxOptions = services.GetOptions<InboxOptions>("inbox");

            if (!inboxOptions.Enabled)
            {
                return services;
            }

            services.AddTransient<IMessageInbox, MongoMessageInbox>();
            services.AddTransient<MongoMessageInbox>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<InboxTypeRegistry>()
                .Register<MongoMessageInbox>();

            return services;
        }
    }
}
