using Edux.Shared.Abstractions.Messaging;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Handlers
{
    internal interface IMessageHandler
    {
        Task HandleAsync<T>(Func<IServiceProvider, T, CancellationToken, Task> handler, T message,
            CancellationToken cancellationToken = default) where T : IMessage;
    }
}
