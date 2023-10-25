using Edux.Shared.Abstractions.Transactions;
using Edux.Shared.Infrastructure.SqlServer.Factories;
using Edux.Shared.Infrastructure.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.SqlServer
{
    public static class Extensions
    {
        public static IServiceCollection AddMsSqlServer(this IServiceCollection services)
        {
            var options = services.GetOptions<SqlServerOptions>("sqlserver");
            services.AddSingleton(options);

            services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();

            services.AddSingleton(new UnitOfWorkTypeRegistry());

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

        public static IServiceCollection AddUnitOfWork<T>(this IServiceCollection services) 
            where T : class, IUnitOfWork
        {
            services.AddScoped<IUnitOfWork, T>();
            services.AddScoped<T>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<UnitOfWorkTypeRegistry>().Register<T>();

            return services;
        }
    }
}
