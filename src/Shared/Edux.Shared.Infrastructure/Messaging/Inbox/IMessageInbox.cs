namespace Edux.Shared.Infrastructure.Messaging.Inbox
{
    internal interface IMessageInbox
    {
        Task HandleAsync(string messageId, string messageName, Func<Task> handler, 
            CancellationToken cancellationToken = default);

        Task CleanUpAsync(DateTime? to = null, CancellationToken cancellationToken = default);
    }
}
