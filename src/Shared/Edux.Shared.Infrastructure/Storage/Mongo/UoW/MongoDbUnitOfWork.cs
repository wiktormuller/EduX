using Edux.Shared.Abstractions.SharedKernel.Types;
using Edux.Shared.Abstractions.Transactions;
using Edux.Shared.Infrastructure.Storage.Mongo.Context;
using MongoDB.Driver;

namespace Edux.Shared.Infrastructure.Storage.Mongo.UoW
{
    public abstract class MongoDbUnitOfWork<T> : IUnitOfWork where T : IIdentifiable<T>
    {
        private readonly IMongoClient _mongoClient;
        private readonly IOperationsContext _operationsContext;

        public MongoDbUnitOfWork(IMongoClient mongoClient,
            IOperationsContext operationsContext)
        {
            _mongoClient = mongoClient;
            _operationsContext = operationsContext;
        }

        public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            using var session = await _mongoClient.StartSessionAsync(cancellationToken: cancellationToken);
            session.StartTransaction();

            try
            {
                var operationsTasks = _operationsContext.Operations.Select(operation => operation());
                await Task.WhenAll(operationsTasks);

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
