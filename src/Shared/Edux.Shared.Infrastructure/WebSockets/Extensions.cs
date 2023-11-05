using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.WebSockets
{
    public static class Extensions
    {
        public static IServiceCollection AddSignalr(this IServiceCollection services)
        {
            var options = services.GetOptions<SignalrOptions>("signalR");
            services.AddSingleton(options);

            var signalr = services.AddSignalR();

            if (!options.Backplane.Equals("redis", StringComparison.InvariantCultureIgnoreCase))
            {
                return services;
            }

            var redisOptions = services.GetOptions<Storage.Redis.RedisOptions>("redis");
            signalr.AddStackExchangeRedis(redisOptions.ConnectionString);

            return services;
        }
    }
}
