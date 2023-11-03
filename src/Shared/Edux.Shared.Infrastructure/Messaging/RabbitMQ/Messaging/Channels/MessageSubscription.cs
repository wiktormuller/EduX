namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Channels
{
    internal class MessageSubscription : IMessageSubscription
    {
        public Type Type { get; }

        public Func<IServiceProvider, object, CancellationToken, Task> Handler { get; }

        public MessageSubscription(Type type, Func<IServiceProvider, object, CancellationToken, Task> handler = null)
        {
            Type = type;
            Handler = handler;
        }
    }
}
