using Edux.Shared.Infrastructure.Api.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Edux.Shared.Infrastructure.Storage.SqlServer.Initializers
{
    internal class DbAppInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbAppInitializerHealthCheck _healthCheck;

        public DbAppInitializer(IServiceProvider serviceProvider,
            DbAppInitializerHealthCheck healthCheck)
        {
            _serviceProvider = serviceProvider;
            _healthCheck = healthCheck;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var dbContextTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly =>
                    !string.Equals(assembly.FullName,
                        "Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5",
                        StringComparison.OrdinalIgnoreCase)) // https://github.com/dotnet/SqlClient/issues/1930
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsInterface && t != typeof(DbContext));

            using var scope = _serviceProvider.CreateScope();
            foreach (var dbContextType in dbContextTypes)
            {
                var dbContext = scope.ServiceProvider.GetService(dbContextType) as DbContext;
                if (dbContext is null || dbContext.GetType().Name.Contains("Read")) // Convention
                {
                    continue;
                }

                await dbContext.Database.MigrateAsync(cancellationToken);
            }

            _healthCheck.StartupCompleted = true;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
