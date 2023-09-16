using Edux.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal static class Extensions
    {
        public static IServiceCollection AddContext(this IServiceCollection services)
        {
            services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            services.AddTransient(sp => sp.GetRequiredService<ICorrelationContextAccessor>().CorrelationContext);

            return services;
        }

        public static IApplicationBuilder UseContext(this IApplicationBuilder app)
        {
            app.Use((httpContext, next) =>
            {
                httpContext.RequestServices.GetRequiredService<ICorrelationContextAccessor>()
                    .CorrelationContext = new CorrelationContext(httpContext);

                return next();
            });

            return app;
        }
    }
}
