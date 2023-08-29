﻿using Edux.Shared.Abstractions.Crypto;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Commands;
using Edux.Shared.Infrastructure.Crypto;
using Edux.Shared.Infrastructure.Events;
using Edux.Shared.Infrastructure.Queries;
using Edux.Shared.Infrastructure.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Edux.Bootstrapper")]
namespace Edux.Shared.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IList<Assembly> assemblies)
        {
            services.AddSingleton<IClock, UtcClock>();
            services.AddQueries(assemblies);
            services.AddCommands(assemblies);
            services.AddEvents(assemblies);
            services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
            services.AddHttpContextAccessor();

            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            //app.UseErrorHandling(); // TODO: Implement error handling
            //app.UseSwagger();
            //app.UseSwaggerUI();
            app.UseReDoc(reDoc =>
            {
                reDoc.RoutePrefix = "docs";
                reDoc.SpecUrl("/swagger/v1/swagger.json");
                reDoc.DocumentTitle = "Confab API";
            });
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            return app;
        }
    }
}
