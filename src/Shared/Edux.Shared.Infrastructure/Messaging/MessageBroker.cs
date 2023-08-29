using Convey.MessageBrokers;
using Edux.Shared.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging
{
    internal sealed class MessageBroker : IMessageBroker
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ILogger<MessageBroker> _logger;

        public MessageBroker(IBusPublisher busPublisher,
            ILogger<MessageBroker> logger)
        {
            _busPublisher = busPublisher;
            _logger = logger;
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

                await _busPublisher.PublishAsync(message);
            }
        }
    }
}
