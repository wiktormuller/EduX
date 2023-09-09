using Edux.Shared.Abstractions.Messaging;

namespace Edux.Shared.Infrastructure.Messaging.Outbox
{
    internal interface IMessageOutbox
    {
        Task SaveAsync<T>(T message, string messageId = null, object messageContext = null) where T : IMessage;
        Task PublishUnsentAsync();
        Task CleanupAsync(DateTime? to = null);
    }
}
