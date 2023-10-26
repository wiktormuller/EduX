using RabbitMQ.Client;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Connections
{
    internal class ProducerConnection
    {
        public IConnection Connection { get; }

        public ProducerConnection(IConnection connection)
        {
            Connection = connection;
        }
    }
}
