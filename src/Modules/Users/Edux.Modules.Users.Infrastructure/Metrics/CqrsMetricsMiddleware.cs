using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Queries;
using Edux.Shared.Abstraction.Observability.Metrics;
using Edux.Shared.Infrastructure.Observability.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

namespace Edux.Modules.Users.Infrastructure.Metrics
{
    [Meter("cqrs")]
    internal sealed class CqrsMetricsMiddleware : IMiddleware
    {
        private static readonly Meter Meter = new("cqrs");

        private static readonly Counter<long> HandledUseCasesCounter =
            Meter.CreateCounter<long>("handled_cqrs");

        private readonly bool _enabled;

        private readonly IDictionary<string, KeyValuePair<string, object?>[]> _metrics = 
            new Dictionary<string, KeyValuePair<string, object?>[]>
            {
                ["POST:/sign-in"] = CreateTagsForCommand(typeof(SignIn).Name),
                ["POST:/sign-up"] = CreateTagsForCommand(typeof(SignUp).Name),

                ["POST:/refresh-tokens/use"] = CreateTagsForCommand(typeof(UseRefreshToken).Name),
                ["POST:/refresh-tokens/revoke"] = CreateTagsForCommand(typeof(RevokeRefreshToken).Name),

                ["PUT:/users/{id}"] = CreateTagsForCommand(typeof(UpdateUser).Name),
                ["GET:/users"] = CreateTagsForQuery(typeof(GetUsers).Name),
                ["GET:/users/me"] = CreateTagsForQuery(typeof(GetUserMe).Name),
                // ["GET:/users/id"] = CreateTagsForQuery(typeof(GetUserDetails).Name), // Entrypoint for ModuleClient (in-memory call)
            };

        public CqrsMetricsMiddleware(MetricsOptions options)
        {
            _enabled = options.Enabled;
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
