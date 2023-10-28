namespace Edux.Shared.Infrastructure.Messaging.Inbox.Options
{
    internal class InboxOptions
    {
        public bool Enabled { get; set; }
        public double? CleanupIntervalInHours { get; set; }
    }
}
