using Edux.Shared.Abstractions.Messaging.Outbox;
using Edux.Shared.Infrastructure.Initializers;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using MongoDB.Driver;

namespace Edux.Shared.Infrastructure.Messaging.Outbox.Mongo
{
    internal sealed class MongoOutboxInitializer : IInitializer
    {
        private readonly IMongoDatabase _database;
        private readonly OutboxOptions _options;

        public MongoOutboxInitializer(IMongoDatabase database,
            OutboxOptions options)
        {
            _database = database;
            _options = options;
        }

        public async Task InitializeAsync()
        {
            if (!_options.Enabled)
            {
                return;
            }

            var outboxBuilder = Builders<OutboxMessage>.IndexKeys;
            await _database.GetCollection<OutboxMessage>("outbox")
                .Indexes.CreateOneAsync(
                    new CreateIndexModel<OutboxMessage>(outboxBuilder.Ascending(i => i.CreatedAt)));
        }
    }
}
