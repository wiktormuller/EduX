using RabbitMQ.Client;

namespace Edux.Shared.Infrastructure.RabbitMQ.Connections
{
    internal interface IChannelFactory
    {
        IModel Create(IConnection connection);
    }
}
