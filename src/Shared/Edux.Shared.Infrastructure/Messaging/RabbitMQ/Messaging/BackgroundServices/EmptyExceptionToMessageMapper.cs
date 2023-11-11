using Edux.Shared.Abstraction.Messaging;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.BackgroundServices
{
    internal class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
    {
        public object? Map(Exception exception, object message) => null;
    }
}
