using Edux.Modules.Notifications.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Edux.Shared.Infrastructure.Messaging.MailKit;
using Edux.Shared.Infrastructure;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ;
using Edux.Modules.Notifications.Messages.Events;
using Edux.Modules.Notifications.Hubs;
using Edux.Shared.Infrastructure.Storage.Mongo;
using Edux.Shared.Infrastructure.Messaging;
using Edux.Shared.Infrastructure.Storage;
using Edux.Modules.Notifications.UoW;

namespace Edux.Modules.Notifications
{
    internal static class Extensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<IMessageService, MessageService>();

            var options = services.GetOptions<MailKitOptions>("mailkit");
            services.AddSingleton(options);

            services.AddTransient<IHubService, HubService>();
            services.AddTransient<IHubWrapper, HubWrapper>();

            services.AddMongo();

            services.AddUnitOfWork<NotificationsUnitOfWork>();

            services.AddMongoOutbox<NotificationsModule>();
            services.AddMongoInbox();


            return services;
        }

        public static IApplicationBuilder UseCore(this  IApplicationBuilder app)
        {
            app.UseRabbitMq()
                .SubscribeForEvent<SignedUp>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationsHub>(NotificationsModule.BasePath + "/notifications");
            });

            return app;
        }

        internal static string ToUserGroup(this Guid userId)
        {
            return userId.ToString("N").ToUserGroup();
        }

        internal static string ToUserGroup(this string userId)
        {
            return $"users:{userId}";
        }
    }
}
