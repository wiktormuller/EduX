using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Subscribers;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Subscribers
{
    internal sealed class RabbitMqMessageSubscriber : IMessageSubscriber
    {
        private readonly MessageSubscriptionsChannel _messageSubscribersChannel;

        public RabbitMqMessageSubscriber(MessageSubscriptionsChannel messageSubscribersChannel)
        {
            _messageSubscribersChannel = messageSubscribersChannel;
        }

        public IMessageSubscriber SubscribeForCommand<TCommand>() where TCommand : class, ICommand
        {
            return SubscribeForMessage<TCommand>((serviceProvider, command, cancellationToken) => 
                serviceProvider
                    .GetRequiredService<ICommandDispatcher>()
                    .SendAsync(command, cancellationToken));
        }

        public IMessageSubscriber SubscribeForEvent<TEvent>() where TEvent : class, IEvent
        {
            return SubscribeForMessage<TEvent>((serviceProvider, integrationEvent, cancellationToken) =>
                serviceProvider
                    .GetRequiredService<IEventDispatcher>()
                    .PublishAsync(integrationEvent, cancellationToken));
        }

        public IMessageSubscriber SubscribeForMessage<T>(Func<IServiceProvider, T, CancellationToken, Task> handler)
            where T : class, IMessage
        {
            var type = typeof(T);
            var messageSubscription = new MessageSubscription(
                type, 
                (serviceProvider, message, cancellationToken) => handler(serviceProvider, (T)message, cancellationToken));

            _messageSubscribersChannel.Writer.TryWrite(messageSubscription);

            return this;
        }
    }
}
