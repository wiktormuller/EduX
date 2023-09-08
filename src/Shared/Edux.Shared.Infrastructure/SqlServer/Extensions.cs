using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.SqlServer
{
    public static class Extensions
    {
        public static IServiceCollection AddMsSqlServer<T>(this IServiceCollection services) where T : DbContext
        {
            var options = services.GetOptions<SqlServerOptions>("sqlserver");
            services.AddDbContext<T>(dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(options.ConnectionString));

            services.AddScoped<IUnitOfWork, SqlServerUnitOfWork<T>>();

            return services;
        }
    }
}
