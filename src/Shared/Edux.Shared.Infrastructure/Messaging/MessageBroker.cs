﻿using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Infrastructure.Messaging.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging
{
    internal sealed class MessageBroker : IMessageBroker
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ILogger<MessageBroker> _logger;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly OutboxTypeRegistry _outboxTypeRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _isOutboxEnabled;

        public MessageBroker(IBusPublisher busPublisher,
            ILogger<MessageBroker> logger,
            ICorrelationContextAccessor correlationContextAccessor,
            OutboxTypeRegistry outboxTypeRegistry,
            IServiceProvider serviceProvider,
            OutboxOptions options)
        {
            _busPublisher = busPublisher;
            _logger = logger;
            _correlationContextAccessor = correlationContextAccessor;
            _outboxTypeRegistry = outboxTypeRegistry;
            _serviceProvider = serviceProvider;
            _isOutboxEnabled = options.Enabled;
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


                if (_isOutboxEnabled)
                {
                    var outboxType = _outboxTypeRegistry.Resolve(message) 
                        ?? throw new InvalidOperationException($"Outbox is not registered for module: '{message.GetModuleName()}'.");
                    
                    using var scope = _serviceProvider.CreateScope();
                    var messageOutbox = (IMessageOutbox)scope.ServiceProvider.GetRequiredService(outboxType);

                    await messageOutbox.SaveAsync(message, messageId, _correlationContextAccessor.CorrelationContext);
                    return;
                }

                await _busPublisher.PublishAsync(message, messageId: messageId, 
                    messageContext: _correlationContextAccessor.CorrelationContext);
            }
        }
    }
}
