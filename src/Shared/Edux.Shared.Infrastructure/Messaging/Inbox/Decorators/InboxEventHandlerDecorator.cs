using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Messaging.Contexts;
using Edux.Shared.Infrastructure.Decorator;
using Edux.Shared.Infrastructure.Messaging.Inbox.Options;
using Humanizer;
using System.Collections.Concurrent;

namespace Edux.Shared.Infrastructure.Messaging.Inbox.Decorators
{
    [Decorator]
    internal sealed class InboxEventHandlerDecorator<T> : IEventHandler<T> where T : class, IEvent
    {
        private static readonly ConcurrentDictionary<Type, string> Names = new();

        private readonly IEventHandler<T> _eventHandler;
        private readonly IMessageContextProvider _messageContextProvider;
        private readonly IMessageInbox _inbox;
        private readonly InboxOptions _inboxOptions;

        public InboxEventHandlerDecorator(IEventHandler<T> eventHandler,
            IMessageContextProvider messageContextProvider,
            IMessageInbox inbox,
            InboxOptions inboxOptions)
        {
            _eventHandler = eventHandler;
            _messageContextProvider = messageContextProvider;
            _inbox = inbox;
            _inboxOptions = inboxOptions;
        }

        public async Task HandleAsync(T @event, CancellationToken cancellationToken)
        {
            var messageContext = _messageContextProvider.GetCurrent();

            var messageName = Names.GetOrAdd(typeof(T), typeof(T).Name.Underscore());
            var handlerAction = () => _eventHandler.HandleAsync(@event, cancellationToken);
            
            if (_inboxOptions.Enabled && messageContext.MessageId is not null)
            {
                await _inbox.HandleAsync(messageContext.MessageId, messageName, 
                    () => _eventHandler.HandleAsync(@event, cancellationToken));
            }

            await _eventHandler.HandleAsync(@event, cancellationToken);
        }
    }
}
