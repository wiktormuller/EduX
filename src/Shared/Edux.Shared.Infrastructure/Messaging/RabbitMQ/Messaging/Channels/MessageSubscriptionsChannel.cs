using System.Threading.Channels;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Channels
{
    internal sealed class MessageSubscriptionsChannel
    {
        private readonly Channel<IMessageSubscription> _channel 
            = Channel.CreateUnbounded<IMessageSubscription>();

        public ChannelReader<IMessageSubscription> Reader 
            => _channel.Reader;
        public ChannelWriter<IMessageSubscription> Writer 
            => _channel.Writer;
    }
}
