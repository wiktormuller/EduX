using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Events;

namespace Edux.Shared.Abstractions.Messaging.Subscribers
{
    public interface IMessageSubscriber
    {
        public IMessageSubscriber Message<T>(Func<IServiceProvider, T, CancellationToken, Task> handler)
            where T : class, IMessage;

        public IMessageSubscriber Command<T>() where T : class, ICommand;

        public IMessageSubscriber Event<T>() where T : class, IEvent;
    }
}
