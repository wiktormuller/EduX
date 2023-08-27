using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Infrastructure.EF.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Edux.Modules.Users.Api")]
namespace Edux.Modules.Users.Infrastructure
{
    internal static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, SqlServerUserRepository>();
            services.AddScoped<IRefreshTokenRepository, SqlServerRefreshTokenRepository>();
            //services.AddSqlServer<UsersReadDbContext>(); // TODO: Implement shared infrastructure for sql server
            //services.AddSqlServer<UsersWriteDbContext>();

            return services;
        }
    }
}