namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions
{
    internal interface IConventionsBuilder
    {
        string GetRoutingKey(Type type);
        string GetExchange(Type type);
        string GetQueue(Type type);
    }
}
