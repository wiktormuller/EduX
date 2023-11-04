using Edux.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal sealed class Context : IContext
    {
        public string TraceId { get; }
        public Guid CorrelationId { get; }
        public IRequestContext RequestContext { get; }
        public IIdentityContext IdentityContext { get; }
        public IMessageContext MessageContext { get; private set; }

        public Context(string messageId, IDictionary<string, object> headers, long timestamp)
            : this(Guid.NewGuid(), Guid.NewGuid().ToString("N"), null, null, new MessageContext(messageId, headers, timestamp))
        {
        }

        public Context(HttpContext context)
            : this(context.TryGetCorrelationId(), context.TraceIdentifier,
                new RequestContext(Guid.NewGuid(), context.GetUserIpAddress(), context.Request.Headers["user-agent"]),
                new IdentityContext(context.User))
        {
        }

        public Context(Guid? correlationId, string traceId, IRequestContext requestContext = null,
            IIdentityContext identityContext = null, IMessageContext messageContext = null)
        {
            CorrelationId = correlationId ?? Guid.NewGuid();
            TraceId = traceId;
            RequestContext = requestContext;
            IdentityContext = identityContext;
            MessageContext = messageContext;
        }

        public void SetMessageContext(IMessageContext messageContext)
        {
            MessageContext = messageContext;
        }
    }
}
