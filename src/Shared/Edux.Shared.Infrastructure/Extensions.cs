using Edux.Shared.Abstractions.Crypto;
using Edux.Shared.Abstractions.Modules;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Api;
using Edux.Shared.Infrastructure.Auth;
using Edux.Shared.Infrastructure.Commands;
using Edux.Shared.Infrastructure.Crypto;
using Edux.Shared.Infrastructure.Events;
using Edux.Shared.Infrastructure.Exceptions;
using Edux.Shared.Infrastructure.Messaging;
using Edux.Shared.Infrastructure.Modules;
using Edux.Shared.Infrastructure.Queries;
using Edux.Shared.Infrastructure.RabbitMQ;
using Edux.Shared.Infrastructure.RabbitMQ.Initializers;
using Edux.Shared.Infrastructure.Services;
using Edux.Shared.Infrastructure.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Edux.Bootstrapper")]
[assembly: InternalsVisibleTo("Edux.Shared.Tests")]
namespace Edux.Shared.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
            IList<Assembly> assemblies, IList<IModule> modules)
        {
            services.AddErrorHandling();

            services.AddAuth(modules);
            services.AddSingleton<IClock, UtcClock>();
            services.AddQueries(assemblies);
            services.AddCommands(assemblies);
            services.AddEvents(assemblies);
            services.AddMessaging();
            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddHttpContextAccessor();
            services.AddRabbitMq();

            services.AddHostedService<AppInitializer>();

            services.AddModuleInfo(modules);
            services.AddControllersWithOpenApi();

            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseInitializers();
            app.UseCors("cors");
            app.UseErrorHandling();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseReDoc(reDoc =>
            {
                reDoc.RoutePrefix = "docs";
                reDoc.SpecUrl("/swagger/v1/swagger.json");
                reDoc.DocumentTitle = "Edux API";
            });
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            return app;
        }

        public static IApplicationBuilder UseInitializers(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var startupInitializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
            Task.Run(() => startupInitializer.InitializeAsync()).GetAwaiter().GetResult();

            return app;
        }

        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            return configuration.GetOptions<T>(sectionName);
        }

        public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
        {
            var options = new T();
            configuration.GetSection(sectionName).Bind(options);
            return options;
        }
    }
}
