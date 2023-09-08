using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Outbox;
using Edux.Shared.Abstractions.Serializers;
using Edux.Shared.Abstractions.Time;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Edux.Shared.Infrastructure.Messaging.Outbox
{
    internal sealed class EfMessageOutbox<T> : IMessageOutbox where T : DbContext
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IClock _clock;

        private readonly T _dbContext;
        private readonly DbSet<OutboxMessage> _outboxMessageSet;

        public EfMessageOutbox(IJsonSerializer serializer, 
            IClock clock,
            T dbContext)
        {
            _jsonSerializer = serializer;
            _clock = clock;
            _dbContext = dbContext;
            _outboxMessageSet = dbContext.Set<OutboxMessage>();
        }

        public async Task SaveAsync<TMessage>(TMessage message, string messageId = null, object messageContext = null) where TMessage : IMessage
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
    }
}
