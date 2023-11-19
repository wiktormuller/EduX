namespace Edux.Shared.Infrastructure.Messaging.Inbox.Mongo
{
    internal sealed class MongoMessageInbox : IMessageInbox
    {
        public Task CleanUpAsync(DateTime? to = null, CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        public Task HandleAsync(string messageId, string messageName, Func<Task> handler,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }
    }
}
