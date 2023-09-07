using RabbitMQ.Client;

namespace Edux.Shared.Infrastructure.RabbitMQ.Connections
{
    internal sealed class ConsumerConnection
    {
        public IConnection Connection { get; }

        public ConsumerConnection(IConnection connection)
        {
            Connection = connection;
        }
    }
}
