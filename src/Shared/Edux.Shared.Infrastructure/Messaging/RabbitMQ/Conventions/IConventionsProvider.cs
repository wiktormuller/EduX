namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions
{
    internal interface IConventionsProvider
    {
        IConventions Get<T>();
        IConventions Get(Type type);
    }
}
