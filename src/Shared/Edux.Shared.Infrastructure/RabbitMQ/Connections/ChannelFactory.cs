using RabbitMQ.Client;

namespace Edux.Shared.Infrastructure.RabbitMQ.Connections
{
    internal class ChannelFactory : IChannelFactory
    {
        private readonly ChannelAccessor _channelAccessor;

        public ChannelFactory(ChannelAccessor channelAccessor)
        {
            _channelAccessor = channelAccessor;
        }

        public IModel Create(IConnection connection)
        {
            return _channelAccessor.Channel ??= connection.CreateModel();
        }
    }
}
