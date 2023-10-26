namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Contexts
{
    internal interface IMessageContextProvider
    {
        string HeaderName { get; }
        object Get(IDictionary<string, object> headers);
    }
}
