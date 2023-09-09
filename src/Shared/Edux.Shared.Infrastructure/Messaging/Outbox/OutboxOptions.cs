namespace Edux.Shared.Infrastructure.Messaging.Outbox
{
    internal class OutboxOptions
    {
        public bool Enabled { get; set; }
        public int Expiry { get; set; }
        public double IntervalMilliseconds { get; set; }
        public string Type { get; set; } // Sequential | Parallel
        public bool DisableTransactions { get; set; }
    }
}
