using Edux.Shared.Abstractions.Queries;
using Edux.Shared.Abstractions.SharedKernel.Types;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Edux.Shared.Infrastructure.Storage.Mongo.Repositories
{
    internal class MongoRepository<TEntity, TIdentifiable> : IMongoRepository<TEntity, TIdentifiable>
        where TEntity : IIdentifiable<TIdentifiable>
    {
        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            Collection = database.GetCollection<TEntity>(collectionName);
        }

        public IMongoCollection<TEntity> Collection { get; }

        public Task AddAsync(TEntity entity)
        {
            return Collection.InsertOneAsync(entity);
        }

        public Task<PagedResult<TEntity>> BrowseAsync<TQuery>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, TQuery query) 
            where TQuery : IPagedQuery
        {
            return Collection
                .AsQueryable()
                .Where(predicate)
                .PaginateAsync(query);
        }

        public Task DeleteAsync(TIdentifiable id)
        {
            return DeleteAsync(entity => entity.Id.Equals(id));
        }

        public Task DeleteAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return Collection.DeleteOneAsync(predicate);
        }

        public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return Collection
                .Find(predicate)
                .AnyAsync();
        }

        public async Task<IReadOnlyList<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return await Collection
                .Find(predicate)
                .ToListAsync();
        }

        public Task<TEntity> GetAsync(TIdentifiable id)
        {
            return GetAsync(entity => entity.Id.Equals(id));
        }

        public Task<TEntity> GetAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return Collection
                .Find(predicate)
                .SingleOrDefaultAsync();
        }

        public Task UpdateAsync(TEntity entity)
        {
            return UpdateAsync(entity, e => e.Id.Equals(entity.Id));
        }

        public Task UpdateAsync(TEntity entity, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return Collection.ReplaceOneAsync(predicate, entity);
        }
    }
}
