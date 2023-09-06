namespace Edux.Shared.Infrastructure.RabbitMQ.Conventions
{
    internal interface IConventionsProvider
    {
        IConventions Get<T>();
        IConventions Get(Type type);
    }
}
