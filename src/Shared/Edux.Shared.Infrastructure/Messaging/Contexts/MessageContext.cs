using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging.Contexts;

namespace Edux.Shared.Infrastructure.Messaging.Contexts
{
    internal class MessageContext : IMessageContext
    {
        public ICorrelationContext CorrelationContext { get; }
        public string? MessageId { get; }

        public MessageContext(ICorrelationContext correlationContext, string? messageId = null)
        {
            CorrelationContext = correlationContext;
            MessageId = messageId;
        }
    }
}
