using Edux.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddHealthChecks()
                .AddSignalRHub(url: NotificationsModule.BasePath + "/notifications", tags: new[] { "live" });
        }

        public void Use(IApplicationBuilder app)
        {
            app.UseCore();
        }
    }
}
