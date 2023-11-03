namespace Edux.Shared.Abstractions.Contexts
{
    public interface IMessageContext
    {
        public string? MessageId { get; }
        public IDictionary<string, object> Headers { get; }
        public long Timestamp { get; }
    }
}
