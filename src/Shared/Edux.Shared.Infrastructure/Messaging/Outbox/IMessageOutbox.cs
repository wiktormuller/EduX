using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Messaging.Contexts;

namespace Edux.Shared.Infrastructure.Messaging.Outbox
{
    internal interface IMessageOutbox
    {
        Task SaveAsync<T>(T message, string messageId, IMessageContext correlationContext) where T : IMessage;
        Task PublishUnsentAsync();
        Task CleanupAsync(DateTime? to = null);
    }
}
