using Edux.Shared.Abstractions.Queries;
using Edux.Shared.Abstractions.SharedKernel.Types;
using Edux.Shared.Infrastructure.Storage.Mongo.Context;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Edux.Shared.Infrastructure.Storage.Mongo.Repositories
{
    internal class MongoRepository<TEntity, TIdentifiable> : IMongoRepository<TEntity, TIdentifiable>
        where TEntity : IIdentifiable<TIdentifiable>
    {
        private readonly IMongoCollection<TEntity> _dbSet; // a.k.a Collection
        private readonly IOperationsContext _operationsContext;
        private readonly IMongoClient _mongoClient;

        public MongoRepository(IMongoDatabase database,
            string collectionName,
            IOperationsContext operationsContext,
            IMongoClient mongoClient)
        {
            _dbSet = database.GetCollection<TEntity>(collectionName);
            _operationsContext = operationsContext;
            _mongoClient = mongoClient;
        }

        public Task<PagedResult<TEntity>> BrowseAsync<TQuery>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, 
            TQuery query) 
                where TQuery : IPagedQuery
        {
            return _dbSet
                .AsQueryable()
                .Where(predicate)
                .PaginateAsync(query);
        }

        public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet
                .Find(predicate)
                .AnyAsync();
        }

        public async Task<IReadOnlyList<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet
                .Find(predicate)
                .ToListAsync();
        }

        public Task<TEntity> GetAsync(TIdentifiable id)
        {
            return GetAsync(entity => entity.Id.Equals(id));
        }

        public Task<TEntity> GetAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet
                .Find(predicate)
                .SingleOrDefaultAsync();
        }

        public void Add(TEntity entity)
        {
            Func<Task> operation = () => _dbSet.InsertOneAsync(entity);

            _operationsContext.AddOperation(operation);
        }

        public void Delete(TIdentifiable id)
        {
            Delete(entity => entity.Id.Equals(id));
        }

        public void Delete(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            var operation = () => _dbSet.DeleteOneAsync(predicate);

            _operationsContext.AddOperation(operation);
        }

        public void Update(TEntity entity)
        {
            Update(entity, e => e.Id.Equals(entity.Id));
        }

        public void Update(TEntity entity, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            Func<Task> operation = () => _dbSet.ReplaceOneAsync(predicate, entity);

            _operationsContext.AddOperation(operation);
        }

        public async Task SaveChangesAsync()
        {
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var operationsTasks = _operationsContext.Operations.Select(operation => operation());
                await Task.WhenAll(operationsTasks);

                await session.CommitTransactionAsync();
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}
