using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Messaging.Contexts;
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
        private readonly IMessageContextProvider _messageContextProvider;
        private readonly IMessageInbox _messageInbox;
        private readonly InboxOptions _inboxOptions;

        public InboxCommandHandlerDecorator(ICommandHandler<T> commandHandler,
            IMessageContextProvider messageContextProvider,
            IMessageInbox messageInbox,
            InboxOptions inboxOptions)
        {
            _commandHandler = commandHandler;
            _messageContextProvider = messageContextProvider;
            _messageInbox = messageInbox;
            _inboxOptions = inboxOptions;
        }

        public async Task HandleAsync(T command, CancellationToken cancellationToken)
        {
            var context = _messageContextProvider.GetCurrent();
            var messageName = Names.GetOrAdd(typeof(T), typeof(T).Name.Underscore());
            if (_inboxOptions.Enabled && context.MessageId is not null)
            {
                await _messageInbox.HandleAsync(context.MessageId, messageName, 
                    () => _commandHandler.HandleAsync(command, cancellationToken));
            }

            await _commandHandler.HandleAsync(command, cancellationToken);
        }
    }
}
