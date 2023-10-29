using Edux.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Edux.Modules.Notifications
{
    internal sealed class NotificationsModule : IModule
    {
        public const string BasePath = "notifications-module";

        public string Name { get; } = "notifications";
        public string Path => BasePath;

        public IEnumerable<string> Policies { get; } = new[] { "notifications" };

        public void Register(IServiceCollection services)
        {
            services.AddCore();
        }

        public void Use(IApplicationBuilder app)
        {
            app.UseCore();
        }
    }
}
