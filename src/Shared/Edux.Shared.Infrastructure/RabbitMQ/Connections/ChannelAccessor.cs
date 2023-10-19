using RabbitMQ.Client;

namespace Edux.Shared.Infrastructure.RabbitMQ.Connections
{
    internal sealed class ChannelAccessor
    {
        private static readonly ThreadLocal<ChannelHolder> _currentChannel = new(); // Channels per thread

        public IModel? Channel
        {
            get => _currentChannel.Value?.Context;

            set
            {
                var holder = _currentChannel.Value;

                if (holder is not null)
                {
                    holder.Context = null;
                }

                if (value is not null)
                {
                    _currentChannel.Value = new ChannelHolder
                    {
                        Context = value
                    };
                }
            }
        }

        private class ChannelHolder
        {
            public IModel? Context;
        }
    }
}
