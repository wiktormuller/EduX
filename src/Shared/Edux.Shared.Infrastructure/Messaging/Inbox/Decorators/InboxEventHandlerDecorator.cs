using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Events;
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
        private readonly IMessageInbox _inbox;
        private readonly InboxOptions _inboxOptions;
        private readonly IContextProvider _contextProvider;

        public InboxEventHandlerDecorator(IEventHandler<T> eventHandler,
            IMessageInbox inbox,
            InboxOptions inboxOptions,
            IContextProvider contextProvider)
        {
            _eventHandler = eventHandler;
            _inbox = inbox;
            _inboxOptions = inboxOptions;
            _contextProvider = contextProvider;
        }

        public async Task HandleAsync(T @event, CancellationToken cancellationToken)
        {
            var context = _contextProvider.Current();

            var messageName = Names.GetOrAdd(typeof(T), typeof(T).Name.Underscore());
            var handlerAction = () => _eventHandler.HandleAsync(@event, cancellationToken);
            
            if (_inboxOptions.Enabled && context?.MessageContext?.MessageId is not null)
            {
                await _inbox.HandleAsync(context.MessageContext.MessageId, messageName, 
                    () => _eventHandler.HandleAsync(@event, cancellationToken));
            }

            await _eventHandler.HandleAsync(@event, cancellationToken);
        }
    }
}
