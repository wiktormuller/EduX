using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Edux.Shared.Infrastructure.Storage.SqlServer.UoW;

namespace Edux.Modules.Users.Infrastructure.EF
{
    internal class UsersUnitOfWork : SqlServerUnitOfWork<UsersWriteDbContext>
    {
        public UsersUnitOfWork(UsersWriteDbContext dbContext) : base(dbContext)
        {
            
        }
    }
}
