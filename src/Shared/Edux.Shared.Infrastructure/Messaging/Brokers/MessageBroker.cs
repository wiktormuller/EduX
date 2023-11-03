using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Contexts;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Infrastructure.Messaging.Contexts;
using Edux.Shared.Infrastructure.Messaging.Outbox;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using Edux.Shared.Infrastructure.Messaging.Outbox.Registries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging.Brokers
{
    internal sealed class MessageBroker : IMessageBroker
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ILogger<MessageBroker> _logger;
        private readonly IContextAccessor _correlationContextAccessor;
        private readonly OutboxTypeRegistry _outboxTypeRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageContextProvider _messageContextProvider;
        private readonly bool _isOutboxEnabled;

        public MessageBroker(IBusPublisher busPublisher,
            ILogger<MessageBroker> logger,
            IContextAccessor correlationContextAccessor,
            OutboxTypeRegistry outboxTypeRegistry,
            IServiceProvider serviceProvider,
            OutboxOptions options,
            IMessageContextProvider messageContextProvider)
        {
            _busPublisher = busPublisher;
            _logger = logger;
            _correlationContextAccessor = correlationContextAccessor;
            _outboxTypeRegistry = outboxTypeRegistry;
            _serviceProvider = serviceProvider;
            _isOutboxEnabled = options.Enabled;
            _messageContextProvider = messageContextProvider;
        }

        public async Task PublishAsync(params IMessage[] messages)
        {
            if (messages is null)
            {
                return;
            }

            messages = messages.Where(m => m is not null).ToArray();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                var messageId = Guid.NewGuid().ToString("N");
                _logger.LogTrace($"Publishing integration message: {message.GetType().Name} [id: '{messageId}'].");

                var messageContext = new MessageContext(_correlationContextAccessor.CorrelationContext, messageId);
                _messageContextProvider.Set(messageContext);

                if (_isOutboxEnabled)
                {
                    var outboxType = _outboxTypeRegistry.Resolve(message)
                        ?? throw new InvalidOperationException($"Outbox is not registered for module: '{message.GetModuleName()}'.");

                    using var scope = _serviceProvider.CreateScope();
                    var messageOutbox = (IMessageOutbox)scope.ServiceProvider.GetRequiredService(outboxType);

                    await messageOutbox.SaveAsync(message, messageId, messageContext);
                    return;
                }

                await _busPublisher.PublishAsync(message, messageId, messageContext);
            }
        }
    }
}
