using Edux.Shared.Abstractions.Exceptions;
using Edux.Shared.Infrastructure.Exceptions.Dispatchers;
using Edux.Shared.Infrastructure.Exceptions.Mappers;
using Edux.Shared.Infrastructure.Exceptions.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Exceptions
{
    internal static class Extensions
    {
        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            services
                .AddScoped<ErrorHandlerMiddleware>()
                .AddSingleton<IExceptionToResponseMapper, DefaultExceptionToResponseMapper>()
                .AddSingleton<IExceptionDispatcher, ExceptionDispatcher>();

            return services;
        }

        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlerMiddleware>();
            return app;
        }
    }
}
