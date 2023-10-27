namespace Edux.Shared.Abstractions.Messaging.Inbox
{
    public class InboxMessage
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public InboxMessage(string id, string name, DateTime receivedAt)
        {
            Id = id;
            Name = name;
            ReceivedAt = receivedAt;
        }

        public void Process(DateTime processedAt)
        {
            ProcessedAt = processedAt;
        }
    }
}
