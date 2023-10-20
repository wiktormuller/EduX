using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Queries;
using Edux.Shared.Abstraction.Observability.Metrics;
using Edux.Shared.Infrastructure.Observability.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;

namespace Edux.Modules.Users.Infrastructure.Metrics
{
    [Meter("cqrs")]
    internal sealed class CqrsMetricsMiddleware : IMiddleware
    {
        private static readonly Meter Meter = new("cqrs");

        private static readonly Counter<long> HandledUseCasesCounter =
            Meter.CreateCounter<long>("handled_use_cases");

        private readonly bool _enabled;

        private readonly IDictionary<string, KeyValuePair<string, object?>[]> _metrics = 
            new Dictionary<string, KeyValuePair<string, object?>[]>
            {
                // POST:/users-module/users/sign-up
                ["POST:/users-module/users/sign-in"] = CreateTagsForCommand(typeof(SignIn).Name),
                ["POST:/users-module/users/sign-up"] = CreateTagsForCommand(typeof(SignUp).Name),

                ["POST:/users-module/users/refresh-tokens/use"] = CreateTagsForCommand(typeof(UseRefreshToken).Name),
                ["POST:/users-module/users/refresh-tokens/revoke"] = CreateTagsForCommand(typeof(RevokeRefreshToken).Name),

                ["PUT:/users-module/users/users/{id}"] = CreateTagsForCommand(typeof(UpdateUser).Name),
                ["GET:/users-module/users/users"] = CreateTagsForQuery(typeof(GetUsers).Name),
                ["GET:/users-module/users/users/me"] = CreateTagsForQuery(typeof(GetUserMe).Name),
                // ["GET:/users/id"] = CreateTagsForQuery(typeof(GetUserDetails).Name), // Entrypoint for ModuleClient (in-memory call)
            };

        public CqrsMetricsMiddleware(IOptions<MetricsOptions> options)
        {
            _enabled = options.Value.Enabled;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!_enabled)
            {
                return next(context);
            }

            var request = context.Request;
            var key = $"{request.Method}:{request.Path}";

            if (!_metrics.TryGetValue(key, out var metrics))
            {
                return next(context);
            }

            HandledUseCasesCounter.Add(1, metrics);

            return next(context);
        }

        private static KeyValuePair<string, object?>[] CreateTagsForCommand(string commandName)
        {
            return new KeyValuePair<string, object?>[]
            {
                new ("command", commandName)
            };
        }

        private static KeyValuePair<string, object?>[] CreateTagsForQuery(string queryName)
        {
            return new KeyValuePair<string, object?>[]
            {
                new ("query", queryName)
            };
        }
    }
}
