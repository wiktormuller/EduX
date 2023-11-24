using Edux.Shared.Abstractions.Messaging.Inbox;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Messaging.Inbox.Options;
using Edux.Shared.Infrastructure.Storage.Mongo.Repositories;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging.Inbox.Mongo
{
    internal sealed class MongoMessageInbox : IMessageInbox
    {
        private readonly IClock _clock;
        private readonly ILogger<MongoMessageInbox> _logger;
        private readonly InboxOptions _inboxOptions;
        private readonly IMongoRepository<InboxMessage, string> _repository;

        public MongoMessageInbox(IClock clock,
            ILogger<MongoMessageInbox> logger,
            InboxOptions inboxOptions,
            IMongoRepository<InboxMessage, string> repository)
        {
            _clock = clock;
            _logger = logger;
            _inboxOptions = inboxOptions;
            _repository = repository;
        }

        public async Task CleanUpAsync(DateTime? to = null, CancellationToken cancellationToken = default)
        {
            if (!_inboxOptions.Enabled)
            {
                _logger.LogWarning("Outbox is disabled, incoming messages won't be cleaned up.");
                return;
            }

            var dateTo = to ?? _clock.CurrentDate();

            var inboxMessages = await _repository
                .FindAsync(m => m.ReceivedAt <= dateTo);

            if (!inboxMessages.Any())
            {
                _logger.LogInformation($"No received messages found in inbox till: {dateTo}.");
                return;
            }

            _logger.LogInformation($"Found {inboxMessages.Count} received messages in inbox till: {dateTo}, cleaning up...");

            foreach (var inboxMessage in inboxMessages)
            {
                _repository.Delete(inboxMessage.Id);
            }
            await _repository.SaveChangesAsync();

            _logger.LogInformation($"Removed {inboxMessages.Count} received messages from inbox till: {dateTo}.");
        }

        public async Task HandleAsync(string messageId, string messageName, Func<Task> handler,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Received a message with ID: '{messageId}' to be processed.");

            if (await _repository.ExistsAsync(m => m.Id == messageId))
            {
                _logger.LogWarning($"Message with ID: '{messageId}' was already processed.");
                return; // Idempotent action
            }

            _logger.LogInformation($"Processing a message with ID: '{messageId}'...");

            var inboxMessage = new InboxMessage(messageId, messageName, _clock.CurrentDate());

            await handler();

            inboxMessage.Process(_clock.CurrentDate());

            _repository.Add(inboxMessage);

            _logger.LogInformation($"Processed a message with ID: '{messageId}'.");
        }
    }
}
