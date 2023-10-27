using Edux.Shared.Abstractions.Messaging.Inbox;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Messaging.Inbox.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging.Inbox.EF
{
    internal sealed class EfMessageInbox<T> : IMessageInbox where T : DbContext
    {
        private readonly T _dbContext;
        private readonly IClock _clock;
        private readonly DbSet<InboxMessage> _inboxMessages;
        private readonly ILogger<EfMessageInbox<T>> _logger;
        private readonly InboxOptions _inboxOptions;

        public EfMessageInbox(T dbContext,
            IClock clock,
            ILogger<EfMessageInbox<T>> logger,
            InboxOptions inboxOptions)
        {
            _dbContext = dbContext;
            _clock = clock;
            _inboxMessages = dbContext.Set<InboxMessage>();
            _logger = logger;
            _inboxOptions = inboxOptions;
        }

        public async Task HandleAsync(string messageId, string messageName, Func<Task> handler, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Received a message with ID: '{messageId}' to be processed.");

            if (await _inboxMessages.AnyAsync(m => m.Id == messageId, cancellationToken))
            {
                _logger.LogWarning($"Message with ID: '{messageId}' was already processed.");
                return; // Idempotent action
            }

            _logger.LogInformation($"Processing a message with ID: '{messageId}'...");

            var inboxMessage = new InboxMessage(messageId, messageName, _clock.CurrentDate());

            await handler();

            inboxMessage.Process(_clock.CurrentDate());

            await _inboxMessages.AddAsync(inboxMessage);
            await _dbContext.SaveChangesAsync(cancellationToken); // TODO: Should it be removed?

            _logger.LogInformation($"Processed a message with ID: '{messageId}'.");
        }

        public async Task CleanUpAsync(DateTime? to = null, CancellationToken cancellationToken = default)
        {
            if (!_inboxOptions.Enabled)
            {
                _logger.LogWarning("Outbox is disabled, incoming messages won't be cleaned up.");
                return;
            }

            var dateTo = to ?? _clock.CurrentDate();

            var inboxMessages = await _inboxMessages
                .Where(m => m.ReceivedAt <= dateTo)
                .ToListAsync(cancellationToken);

            if (!inboxMessages.Any())
            {
                _logger.LogInformation($"No received messages found in inbox till: {dateTo}.");
                return;
            }

            _logger.LogInformation($"Found {inboxMessages.Count} received messages in inbox till: {dateTo}, cleaning up...");

            _inboxMessages.RemoveRange(inboxMessages);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Removed {inboxMessages.Count} received messages from inbox till: {dateTo}.");
        }
    }
}
