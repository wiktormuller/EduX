namespace Edux.Shared.Infrastructure.SqlServer
{
    internal interface IUnitOfWork
    {
        Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
    }
}
