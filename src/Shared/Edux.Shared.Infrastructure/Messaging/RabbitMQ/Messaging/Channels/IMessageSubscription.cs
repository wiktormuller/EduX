namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Channels
{
    internal interface IMessageSubscription
    {
        Type Type { get; }
        Func<IServiceProvider, object, CancellationToken, Task> Handler { get; }
    }
}
