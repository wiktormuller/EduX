namespace Edux.Shared.Abstractions.Transactions
{
    public interface IUnitOfWork
    {
        Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
    }
}
