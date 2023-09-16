using Edux.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal sealed class CorrelationContext : ICorrelationContext
    {
        public Guid RequestId { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; }
        public string TraceId { get; }
        public string IpAddress { get; }
        public string UserAgent { get; }
        public IIdentityContext Identity { get; }

        public CorrelationContext() : this(Guid.NewGuid(), $"{Guid.NewGuid():N}", null)
        {
        }

        public CorrelationContext(HttpContext context) : this(context.TryGetCorrelationId(), context.TraceIdentifier,
            new IdentityContext(context.User), context.GetUserIpAddress(),
            context.Request.Headers["user-agent"])
        {
        }

        public CorrelationContext(Guid? correlationId, string traceId, IIdentityContext identity = null, string ipAddress = null,
        string userAgent = null)
        {
            CorrelationId = correlationId ?? Guid.NewGuid();
            TraceId = traceId;
            Identity = identity ?? IdentityContext.Empty;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }

        public static ICorrelationContext Empty => new CorrelationContext();
    }
}
