using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Subscribers;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Handlers;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Subscribers
{
    internal sealed class RabbitMqMessageSubscriber : IMessageSubscriber
    {
        private readonly IMessageHandler _messageHandler;
        //private readonly IMessageTypeRegistry _messageTypeRegistry;

        public RabbitMqMessageSubscriber(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public IMessageSubscriber Command<T>() where T : class, ICommand
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        public IMessageSubscriber Event<T>() where T : class, IEvent
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        public IMessageSubscriber Message<T>(Func<IServiceProvider, T, CancellationToken, Task> handler)
            where T : class, IMessage
        {
            //_messageTypeRegistry.Register<T>();

            var messageAttribute = typeof(T).GetCustomAttribute<MessageAttribute>() ?? new MessageAttribute();

            // TODO: Implement
            throw new NotImplementedException();
        }
    }
}
