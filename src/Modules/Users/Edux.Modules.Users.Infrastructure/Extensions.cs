using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Infrastructure.EF;
using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Edux.Modules.Users.Infrastructure.EF.Repositories;
using Edux.Modules.Users.Infrastructure.Metrics;
using Edux.Shared.Infrastructure.Messaging;
using Edux.Shared.Infrastructure.SqlServer;
using Microsoft.AspNetCore.Builder;
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

            services.AddMsSqlServer<UsersReadDbContext>();
            services.AddMsSqlServer<UsersWriteDbContext>();

            services.AddUnitOfWork<UsersUnitOfWork>();

            services.AddOutbox<UsersWriteDbContext>();

            services.AddSingleton<CqrsMetricsMiddleware>();

            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CqrsMetricsMiddleware>();
        }
    }
}