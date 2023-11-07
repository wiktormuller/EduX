using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using System.Threading.RateLimiting;

namespace Edux.Shared.Infrastructure.Api.RateLimiting
{
    internal static class Extensions
    {
        public const string _jwtPolicyName = "jwt";

        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            var options = services.GetOptions<RateLimitOptions>("rateLimiting");
            services.AddSingleton(options);

            services.AddRateLimiter(limiterOptions =>
            {
                limiterOptions.OnRejected = (context, _) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = 
                            ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");
                    }

                    return new ValueTask();
                };

                // limiterOptions.GlobalLimiter = PartitionedRateLimiter.CreateChained

                limiterOptions.AddPolicy(policyName: _jwtPolicyName, partitioner: httpContext =>
                {
                    var accessToken = httpContext.Features
                        .Get<IAuthenticateResultFeature>()
                        ?.AuthenticateResult
                        ?.Properties
                        ?.GetTokenValue("AccessToken")
                        ?.ToString() 
                            ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        return RateLimitPartition.GetFixedWindowLimiter(accessToken, _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                Window = TimeSpan.FromSeconds(options.WindowInSeconds),
                                PermitLimit = options.PermitLimit,
                                QueueLimit = options.QueueLimit,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                            });
                    }

                    return RateLimitPartition.GetFixedWindowLimiter("Anonymous", _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromSeconds(options.WindowInSeconds),
                            PermitLimit = options.PermitLimit,
                            QueueLimit = options.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                });
            });

            return services;
        }
    }
}
