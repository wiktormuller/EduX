using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Infrastructure.Decorator;
using Edux.Shared.Infrastructure.Messaging.Inbox.Options;
using Humanizer;
using System.Collections.Concurrent;

namespace Edux.Shared.Infrastructure.Messaging.Inbox.Decorators
{
    [Decorator]
    internal sealed class InboxCommandHandlerDecorator<T> : ICommandHandler<T> where T : class, ICommand
    {
        private static readonly ConcurrentDictionary<Type, string> Names = new();

        private readonly ICommandHandler<T> _commandHandler;
        private readonly IMessageInbox _messageInbox;
        private readonly InboxOptions _inboxOptions;
        private readonly IContextProvider _contextProvider;

        public InboxCommandHandlerDecorator(ICommandHandler<T> commandHandler,
            IMessageInbox messageInbox,
            InboxOptions inboxOptions,
            IContextProvider contextProvider)
        {
            _commandHandler = commandHandler;
            _messageInbox = messageInbox;
            _inboxOptions = inboxOptions;
            _contextProvider = contextProvider;
        }

        public async Task HandleAsync(T command, CancellationToken cancellationToken)
        {
            var context = _contextProvider.Current();
            var messageName = Names.GetOrAdd(typeof(T), typeof(T).Name.Underscore());
            if (_inboxOptions.Enabled && context?.MessageContext.MessageId is not null)
            {
                await _messageInbox.HandleAsync(context.MessageContext.MessageId, messageName, 
                    () => _commandHandler.HandleAsync(command, cancellationToken));
            }

            await _commandHandler.HandleAsync(command, cancellationToken);
        }
    }
}
