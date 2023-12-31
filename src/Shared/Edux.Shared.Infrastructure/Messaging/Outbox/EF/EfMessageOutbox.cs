﻿using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Outbox;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Abstractions.Serializers;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using Edux.Shared.Infrastructure.Messaging.Outbox.Processors;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging.Outbox.EF
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
        private readonly IContextProvider _contextProvider;

        public EfMessageOutbox(IJsonSerializer serializer,
            IClock clock,
            T dbContext,
            ILogger<EfMessageOutbox<T>> logger,
            IBusPublisher busPublisher,
            OutboxOptions outboxOptions,
            IContextProvider contextProvider)
        {
            _jsonSerializer = serializer;
            _clock = clock;
            _dbContext = dbContext;
            _outboxMessageSet = dbContext.Set<OutboxMessage>();
            _logger = logger;
            _busPublisher = busPublisher;
            _outboxOptions = outboxOptions;
            _contextProvider = contextProvider;
        }

        public async Task SaveAsync<TMessage>(TMessage message, string messageId, IMessageContext messageContext)
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
        }

        public async Task CleanupAsync(DateTime? to = null)
        {
            var module = _dbContext.GetModuleName();

            var dateTo = to ?? _clock.CurrentDate();
            var sentMessages = await _outboxMessageSet
                .Where(o => o.SentAt != null &&
                    o.CreatedAt <= dateTo)
                .ToListAsync();

            if (!sentMessages.Any())
            {
                _logger.LogTrace($"No sent messages found in outbox ('{module}') till: {dateTo}.");
                return;
            }

            _logger.LogTrace($"Found {sentMessages.Count} sent messages in outbox ('{module}') till: {dateTo}, cleaning up...");

            _outboxMessageSet.RemoveRange(sentMessages);
            await _dbContext.SaveChangesAsync();

            _logger.LogTrace($"Found {sentMessages.Count} sent messages in outbox ('{module}') till: {dateTo}, cleaning up...");
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
                var messageContext = _jsonSerializer.Deserialize<IMessageContext>(outboxMessage.Context)
                    ?? throw new ArgumentNullException("Message context was null while processing outboxMessage");

                _contextProvider.Current().SetMessageContext(messageContext);

                _logger.LogInformation($"Publishing a message from outbox ('{module}'): {outboxMessage.Name} [Message ID: {outboxMessage.Id}]...");
                await _busPublisher.PublishAsync(outboxMessage, outboxMessage.Id, messageContext);

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
