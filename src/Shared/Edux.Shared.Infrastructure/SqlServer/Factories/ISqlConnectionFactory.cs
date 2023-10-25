using System.Data;

namespace Edux.Shared.Infrastructure.SqlServer.Factories
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetOpenConnection();
    }
}
