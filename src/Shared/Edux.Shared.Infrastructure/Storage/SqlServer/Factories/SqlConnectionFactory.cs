using Edux.Shared.Infrastructure.Storage.SqlServer.Options;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Edux.Shared.Infrastructure.Storage.SqlServer.Factories
{
    public class SqlConnectionFactory : ISqlConnectionFactory, IDisposable
    {
        private IDbConnection? _connection;
        private readonly string _connectionString;

        private bool _disposed = false;

        public SqlConnectionFactory(SqlServerOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public IDbConnection GetOpenConnection()
        {
            if (_connection is null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }

            return _connection;
        }

        public void Dispose()
        {
            Dispose(disposing: true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to take this object off the finalization queue
            // and prevent finalization code for this object from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // If disposing equals true, dispose ALL MANAGED and UNMANAGED resources
                if (disposing)
                {
                    if (_connection is not null && _connection.State == ConnectionState.Open)
                    {
                        _connection.Dispose(); // Dispose managed resource
                    }
                }

                // We can also release unmanaged resources here...

                _disposed = true;
            }
        }

        // Use C# finalizer syntax for finalization code.
        // This finalizer will run only if the Dispose method does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide finalizer in types derived from this class.
        ~SqlConnectionFactory()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of readability and maintainability.
            Dispose(disposing: false);
        }
    }
}
