using Edux.Shared.Abstractions.Contexts;

namespace Edux.Shared.Abstractions.Messaging.Contexts
{
    public interface IMessageContext
    {
        public ICorrelationContext CorrelationContext { get; }
        public string? MessageId { get; }
    }
}
