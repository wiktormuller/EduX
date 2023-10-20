using Edux.Modules.Users.Application;
using Edux.Modules.Users.Application.Contracts.Requests;
using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Application.Queries;
using Edux.Modules.Users.Core;
using Edux.Modules.Users.Infrastructure;
using Edux.Shared.Abstractions.Modules;
using Edux.Shared.Abstractions.Queries;
using Edux.Shared.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Modules.Users.Api
{
    internal class UsersModule : IModule
    {
        public const string BasePath = "users-module";

        public string Name { get; } = "users";
        public string Path => BasePath;

        public IEnumerable<string> Policies { get; } = new[] { "users" };

        public void Register(IServiceCollection services)
        {
            services.AddCore();
            services.AddApplication();
            services.AddInfrastructure();
        }

        public void Use(IApplicationBuilder app)
        {
            app.UseInfrastructure();

            app.UseModuleRequests() // We register here another entry point, similarly like event handlers or controllers
                .RegisterEntryPoint<GetUserDetailsRequest, UserDetailsResponse>("users/id",
                    (request, serviceProvider, cancellationToken)
                        => serviceProvider.GetRequiredService<IQueryDispatcher>()
                            .QueryAsync(new GetUserDetails { UserId = request.UserId }, cancellationToken));
        }
    }
}
