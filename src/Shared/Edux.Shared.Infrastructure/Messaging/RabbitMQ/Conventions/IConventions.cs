namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions
{
    internal interface IConventions
    {
        Type Type { get; }
        string RoutingKey { get; }
        string Exchange { get; }
        string Queue { get; }
    }
}
