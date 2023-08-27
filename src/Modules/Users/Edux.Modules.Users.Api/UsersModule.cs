using Edux.Modules.Users.Application;
using Edux.Modules.Users.Core;
using Edux.Modules.Users.Infrastructure;
using Edux.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Modules.Users.Api
{
    internal class UsersModule : IModule
    {
        public const string BasePath = "users-module";

        public string Name { get; } = "Users";
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
        }
    }
}
