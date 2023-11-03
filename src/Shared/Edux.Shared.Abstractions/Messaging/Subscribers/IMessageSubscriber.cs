using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Events;

namespace Edux.Shared.Abstractions.Messaging.Subscribers
{
    public interface IMessageSubscriber
    {
        public IMessageSubscriber SubscribeForMessage<T>(Func<IServiceProvider, T, CancellationToken, Task> handler)
            where T : class, IMessage;

        public IMessageSubscriber SubscribeForCommand<T>() where T : class, ICommand;

        public IMessageSubscriber SubscribeForEvent<T>() where T : class, IEvent;
    }
}
