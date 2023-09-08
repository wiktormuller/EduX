namespace Edux.Shared.Abstractions.Messaging.Outbox
{
    public class OutboxMessage // TODO: Extend this object with data that message can contain, like correlationId, spanContext, headers etc.
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
