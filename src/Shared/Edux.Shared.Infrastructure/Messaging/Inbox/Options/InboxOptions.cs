namespace Edux.Shared.Infrastructure.Messaging.Inbox.Options
{
    internal class InboxOptions
    {
        public bool Enabled { get; set; }
        public TimeSpan? CleanupInterval { get; set; }
    }
}
