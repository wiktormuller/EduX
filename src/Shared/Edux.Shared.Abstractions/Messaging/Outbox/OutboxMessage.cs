using Edux.Shared.Abstractions.SharedKernel.Types;

namespace Edux.Shared.Abstractions.Messaging.Outbox
{
    public class OutboxMessage : IIdentifiable<string>
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string Data { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
