using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Shared.Infrastructure.Messaging.Outbox
{
    internal interface IMessageOutbox
    {
        Task SaveAsync<T>(T message, string messageId, IMessageContext messageContext) where T : IMessage;
        Task PublishUnsentAsync();
        Task CleanupAsync(DateTime? to = null);
    }
}
