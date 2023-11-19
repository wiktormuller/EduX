using Edux.Shared.Abstractions.SharedKernel.Types;
using Edux.Shared.Abstractions.Transactions;
using MongoDB.Driver;

namespace Edux.Shared.Infrastructure.Storage.Mongo.UoW
{
    public abstract class MongoDbUnitOfWork<T> : IUnitOfWork where T : IIdentifiable<T>
    {
        private readonly IMongoClient _mongoClient;

        public MongoDbUnitOfWork(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            using var session = await _mongoClient.StartSessionAsync(cancellationToken: cancellationToken);
            session.StartTransaction();

            try
            {
                var commandTasks = _commands.Select(command => command());
                await Task.WhenAll(commandTasks);

                await session.CommitTransactionAsync(cancellationToken: cancellationToken);
            }
            catch
            {
                await session.AbortTransactionAsync(cancellationToken: cancellationToken);
                throw;
            }
        }
    }
}
