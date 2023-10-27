using System.Data;

namespace Edux.Shared.Infrastructure.Storage.SqlServer.Factories
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetOpenConnection();
    }
}
