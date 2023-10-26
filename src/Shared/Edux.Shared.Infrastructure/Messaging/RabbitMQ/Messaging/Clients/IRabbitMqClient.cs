using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Clients
{
    internal interface IRabbitMqClient
    {
        void Send(object message, IConventions conventions, string messageId = null, string correlationId = null,
        string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null);
    }
}
