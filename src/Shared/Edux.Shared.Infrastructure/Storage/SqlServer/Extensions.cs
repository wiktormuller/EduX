using Edux.Shared.Abstractions.Transactions;
using Edux.Shared.Infrastructure.Storage.SqlServer.Factories;
using Edux.Shared.Infrastructure.Storage.SqlServer.Options;
using Edux.Shared.Infrastructure.Transactions.Registries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Storage.SqlServer
{
    public static class Extensions
    {
        public static IServiceCollection AddMsSqlServer(this IServiceCollection services)
        {
            var options = services.GetOptions<SqlServerOptions>("sqlserver");
            services.AddSingleton(options);

            services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();

            return services;
        }

        public static IServiceCollection AddMsSqlServer<T>(this IServiceCollection services)
            where T : DbContext
        {
            var options = services.GetOptions<SqlServerOptions>("sqlserver");
            services.AddDbContext<T>(dbContextOptionsBuilder =>
                dbContextOptionsBuilder.UseSqlServer(options.ConnectionString));

            return services;
        }
    }
}
