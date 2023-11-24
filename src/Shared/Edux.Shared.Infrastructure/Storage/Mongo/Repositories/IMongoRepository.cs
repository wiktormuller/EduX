using Edux.Shared.Abstractions.Queries;
using Edux.Shared.Abstractions.SharedKernel.Types;
using System.Linq.Expressions;

namespace Edux.Shared.Infrastructure.Storage.Mongo.Repositories
{
    public interface IMongoRepository<TEntity, in TIdentifiable> 
        where TEntity : IIdentifiable<TIdentifiable>
    {
        Task<TEntity> GetAsync(TIdentifiable id);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate,
            TQuery query) where TQuery : IPagedQuery;
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

        void Add(TEntity entity);
        void Update(TEntity entity);
        void Update(TEntity entity, Expression<Func<TEntity, bool>> predicate);
        void Delete(TIdentifiable id);
        void Delete(Expression<Func<TEntity, bool>> predicate);

        Task SaveChangesAsync();
    }
}
