using Edux.Shared.Abstractions.Contexts;

namespace Edux.Shared.Abstractions.Messaging.Publishers
{
    public interface IBusPublisher
    {
        Task PublishAsync<T>(T message, string messageId, IMessageContext messageContext,
            string? spanContext = null, IDictionary<string, object>? headers = null) where T : class;
    }
}
