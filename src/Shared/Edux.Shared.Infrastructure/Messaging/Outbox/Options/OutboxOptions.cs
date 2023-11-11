namespace Edux.Shared.Infrastructure.Messaging.Outbox.Options
{
    internal class OutboxOptions
    {
        public bool Enabled { get; set; }
        public int Expiry { get; set; }
        public double IntervalMilliseconds { get; set; }
        public string Type { get; set; } = string.Empty;// Sequential | Parallel
        public bool DisableTransactions { get; set; }
        public double OutboxCleanupIntervalMilliseconds { get; set; }
    }
}
