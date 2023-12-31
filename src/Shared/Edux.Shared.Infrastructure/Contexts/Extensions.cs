﻿using Edux.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal static class Extensions
    {
        private const string CorrelationIdKey = "correlation-id";
        private const string UserIpKey = "x-forwarded-for";
        private const string UserAgentKey = "user-agent";

        public static IServiceCollection AddContext(this IServiceCollection services)
        {
            services.AddSingleton<IContextAccessor, ContextAccessor>();
            services.AddSingleton<IContextProvider, ContextProvider>();
                
            return services;
        }

        public static IApplicationBuilder UseContext(this IApplicationBuilder app)
        {
            app.Use((httpContext, next) =>
            {
                httpContext.RequestServices.GetRequiredService<IContextAccessor>()
                    .Context = new Context(httpContext);

                return next();
            });

            return app;
        }

        public static string GetUserIpAddress(this HttpContext httpContext)
        {
            if (httpContext is null)
            {
                return string.Empty;
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            if (httpContext.Request.Headers.TryGetValue(UserIpKey, out var forwardedFor))
            {
                var ipAddresses = forwardedFor.ToString().Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (ipAddresses.Any())
                {
                    ipAddress = ipAddresses[0];
                }
            }

            return ipAddress ?? string.Empty;
        }

        public static string GetUserAgent(this HttpContext httpContext)
        {
            if (httpContext is null)
            {
                return string.Empty;
            }

            httpContext.Request.Headers.TryGetValue(UserAgentKey, out var userAgent);

            var userAgentValue = userAgent.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(userAgentValue))
            {
                return userAgentValue;
            }

            return string.Empty;
        }

        public static Guid? TryGetCorrelationId(this HttpContext context)
            => context.Items.TryGetValue(CorrelationIdKey, out var id)
                ? (Guid)id!
                : null;

        public static void TryAddCorrelationId(this HttpRequestHeaders headers, string correlationId)
            => headers.TryAddWithoutValidation(CorrelationIdKey, correlationId);
    }
}
