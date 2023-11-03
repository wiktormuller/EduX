using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Clients
{
    internal interface IRabbitMqClient
    {
        void Send(object message, IConventions conventions, string messageId, IMessageContext messageContext,
        string spanContext = null, IDictionary<string, object> headers = null);
    }
}
