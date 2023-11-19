using Edux.Shared.Abstractions.Transactions;
using Edux.Shared.Infrastructure.Storage.Redis;
using Edux.Shared.Infrastructure.Transactions.Registries;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Edux.Shared.Infrastructure.Storage
{
    public static class Extensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services)
        {
            var options = services.GetOptions<RedisOptions>("redis");

            // It's default in-memory implementation for IDistributedCache interface
            // (we can also use IMemoryCache which is natively supported by framework)
            // services.AddDistributedMemoryCache();

            services
                .AddSingleton(options)
                .AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(options.ConnectionString)) // Multiplexer should be held and reused
                .AddTransient(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase(options.Database)) // We can use the connection multiplexer
                .AddStackExchangeRedisCache(opt => // Or we can use the IDistributedCache interface (it internally use IConnectionMultiplexer)
                {
                    opt.Configuration = options.ConnectionString;
                    opt.InstanceName = options.Instance;
                });

            return services;
        }

        public static IServiceCollection AddUnitOfWork<T>(this IServiceCollection services)
            where T : class, IUnitOfWork
        {
            services.AddScoped<IUnitOfWork, T>();
            services.AddScoped<T>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<UnitOfWorkTypeRegistry>().Register<T>();

            return services;
        }
    }
}
