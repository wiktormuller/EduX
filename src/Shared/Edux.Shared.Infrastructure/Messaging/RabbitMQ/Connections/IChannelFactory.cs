using RabbitMQ.Client;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Connections
{
    internal interface IChannelFactory
    {
        IModel Create(IConnection connection);
    }
}
