using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Outbox;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Abstractions.Serializers;
using Edux.Shared.Abstractions.SharedKernel.Types;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using Edux.Shared.Infrastructure.Messaging.Outbox.Processors;
using Edux.Shared.Infrastructure.Storage.Mongo.Repositories;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Messaging.Outbox.Mongo
{
    internal sealed class MongoMessageOutbox<T> : IMessageOutbox where T : class
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IClock _clock;
        private readonly ILogger<MongoMessageOutbox<T>> _logger;
        private readonly IBusPublisher _busPublisher;
        private readonly IContextProvider _contextProvider;
        private readonly OutboxOptions _outboxOptions;
        private readonly IMongoRepository<OutboxMessage, string> _repository;

        public MongoMessageOutbox(IJsonSerializer jsonSerializer,
            IClock clock,
            ILogger<MongoMessageOutbox<T>> logger,
            IBusPublisher busPublisher,
            IContextProvider contextProvider,
            OutboxOptions outboxOptions,
            IMongoRepository<OutboxMessage, string> repository)
        {
            _jsonSerializer = jsonSerializer;
            _clock = clock;
            _logger = logger;
            _busPublisher = busPublisher;
            _contextProvider = contextProvider;
            _outboxOptions = outboxOptions;
            _repository = repository;
        }

        public async Task CleanupAsync(DateTime? to = null)
        {
            var module = typeof(T).GetModuleName();

            var dateTo = to ?? _clock.CurrentDate();
            var sentMessages = await _repository
                .FindAsync(o => o.SentAt != null && o.CreatedAt <= dateTo);

            if (!sentMessages.Any())
            {
                _logger.LogTrace($"No sent messages found in outbox ('{module}') till: {dateTo}.");
                return;
            }

            _logger.LogTrace($"Found {sentMessages.Count} sent messages in outbox ('{module}') till: {dateTo}, cleaning up...");

            foreach (var sentMessage in sentMessages)
            {
                _repository.Delete(sentMessage.Id);
            }
            await _repository.SaveChangesAsync();

            _logger.LogTrace($"Found {sentMessages.Count} sent messages in outbox ('{module}') till: {dateTo}, cleaning up...");
        }

        public async Task PublishUnsentAsync()
        {
            var module = typeof(T).GetModuleName();

            var unsentMessages = await _repository
                .FindAsync(message => message.SentAt == null);

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
                _repository.Update(outboxMessage);

                if (_outboxOptions.Type == OutboxType.Sequential.ToString().ToLowerInvariant())
                {
                    await _repository.SaveChangesAsync();
                }
            }

            if (_outboxOptions.Type == OutboxType.Parallel.ToString().ToLowerInvariant())
            {
                await _repository.SaveChangesAsync();
            }
        }

        public async Task SaveAsync<T>(T message, string messageId, IMessageContext messageContext) where T : IMessage
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

            _repository.Add(outboxMessage);
            await _repository.SaveChangesAsync();
        }
    }
}
