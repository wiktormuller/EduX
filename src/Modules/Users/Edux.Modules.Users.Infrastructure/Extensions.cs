using Edux.Modules.Users.Application.Graphql.Messaging;
using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Infrastructure.EF;
using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Edux.Modules.Users.Infrastructure.EF.Repositories;
using Edux.Modules.Users.Infrastructure.Graphql.Services;
using Edux.Modules.Users.Infrastructure.Grpc;
using Edux.Modules.Users.Infrastructure.Metrics;
using Edux.Shared.Infrastructure;
using Edux.Shared.Infrastructure.Messaging;
using Edux.Shared.Infrastructure.Storage.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
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

            services.AddEfOutbox<UsersWriteDbContext>();
            services.AddEfInbox<UsersWriteDbContext>();

            services.AddSingleton<CqrsMetricsMiddleware>();

            services.AddSingleton<IUsersMessageService, UsersMessageService>();

            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseMiddleware<CqrsMetricsMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<UsersService>();
            });

            app.ShareProtoFiles("/users-proto",
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Grpc\\Protos"));

            return app;
        }
    }
}