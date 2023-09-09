using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Outbox;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Abstractions.Serializers;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Messaging.Outbox.Processors;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging.Outbox
{
    internal sealed class EfMessageOutbox<T> : IMessageOutbox where T : DbContext
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IClock _clock;

        private readonly T _dbContext;
        private readonly DbSet<OutboxMessage> _outboxMessageSet;
        private readonly ILogger<EfMessageOutbox<T>> _logger;
        private readonly IBusPublisher _busPublisher;
        private readonly OutboxOptions _outboxOptions;

        public EfMessageOutbox(IJsonSerializer serializer,
            IClock clock,
            T dbContext,
            ILogger<EfMessageOutbox<T>> logger,
            IBusPublisher busPublisher,
            OutboxOptions outboxOptions)
        {
            _jsonSerializer = serializer;
            _clock = clock;
            _dbContext = dbContext;
            _outboxMessageSet = dbContext.Set<OutboxMessage>();
            _logger = logger;
            _busPublisher = busPublisher;
            _outboxOptions = outboxOptions;
        }

        public async Task SaveAsync<TMessage>(TMessage message, string messageId = null, object messageContext = null) 
            where TMessage : IMessage
        {
            if (message is null)
            {
                return;
            }

            var outboxMessage = new OutboxMessage
            {
                Id = messageId,
                Context = _jsonSerializer.Serialize(messageContext),
                Name = message.GetType().Name.Underscore(),
                Data = _jsonSerializer.Serialize((object)message),
                Type = message.GetType().AssemblyQualifiedName,
                CreatedAt = _clock.CurrentDate()
            };

            await _outboxMessageSet.AddAsync(outboxMessage);
            await _dbContext.SaveChangesAsync();
        }

        public Task CleanupAsync(DateTime? to = null)
        {
            return Task.CompletedTask;
        }

        public async Task PublishUnsentAsync()
        {
            var module = _dbContext.GetModuleName();

            var unsentMessages = await _outboxMessageSet
                .Where(x => x.SentAt == null)
                .OrderBy(x => x.SentAt)
                .ToListAsync();

            if (!unsentMessages.Any())
            {
                _logger.LogTrace($"No unsent messages found in outbox ('{module}').");
                return;
            }

            _logger.LogTrace($"Found {unsentMessages.Count} unsent messages in outbox ('{module}'), sending...");

            foreach (var outboxMessage in unsentMessages)
            {
                _logger.LogInformation($"Publishing a message from outbox ('{module}'): {outboxMessage.Name} [Message ID: {outboxMessage.Id}]...");
                await _busPublisher.PublishAsync(outboxMessage, messageId: outboxMessage.Id, messageContext: outboxMessage.Context);

                outboxMessage.SentAt = _clock.CurrentDate();
                _outboxMessageSet.Update(outboxMessage);

                if (_outboxOptions.Type == OutboxType.Sequential.ToString().ToLowerInvariant())
                {
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (_outboxOptions.Type == OutboxType.Parallel.ToString().ToLowerInvariant())
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
