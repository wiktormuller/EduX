using Edux.Shared.Abstractions.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Edux.Shared.Infrastructure.SqlServer.UoW
{
    public abstract class SqlServerUnitOfWork<T> : IUnitOfWork where T : DbContext
    {
        private readonly T _dbContext;

        public SqlServerUnitOfWork(T dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await action();
                await _dbContext.SaveChangesAsync(cancellationToken); // Will create and commit transaction automatically only when there is not one defined manually
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
