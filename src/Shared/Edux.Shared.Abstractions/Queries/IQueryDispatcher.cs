namespace Edux.Shared.Abstractions.Queries
{
    public interface IQueryDispatcher
    {
        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
    }
}
