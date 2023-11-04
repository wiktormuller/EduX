using Edux.Shared.Abstraction.Messaging;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.BackgroundServices
{
    internal class EmptyExceptionToFailedMessageMapper : IExceptionToFailedMessageMapper
    {
        public FailedMessage Map(Exception exception, object message) => null;
    }
}
