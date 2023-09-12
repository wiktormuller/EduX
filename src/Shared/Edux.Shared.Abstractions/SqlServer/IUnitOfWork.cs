namespace Edux.Shared.Abstractions.SqlServer
{
    public interface IUnitOfWork
    {
        Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
    }
}
