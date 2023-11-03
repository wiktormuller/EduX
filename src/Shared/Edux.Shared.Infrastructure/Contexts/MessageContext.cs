using Edux.Shared.Abstractions.Contexts;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal class MessageContext : IMessageContext
    {
        public string MessageId { get; }
        public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();
        public long Timestamp { get; }

        public MessageContext(string messageId, IDictionary<string, object> headers, long timestamp)
        {
            MessageId = messageId;
            Headers = headers;
            Timestamp = timestamp;
        }
    }
}
