using Edux.Modules.Notifications.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Edux.Shared.Infrastructure.Messaging.MailKit;
using Edux.Shared.Infrastructure;

namespace Edux.Modules.Notifications
{
    internal static class Extensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<IMessageService, MessageService>();

            var options = services.GetOptions<MailKitOptions>("mailkit");
            services.AddSingleton(options);

            return services;
        }

        public static IApplicationBuilder UseCore(this  IApplicationBuilder app)
        {
            //app.UseRabbitMq(); // TODO: Add subscriber

            return app;
        }
    }
}
