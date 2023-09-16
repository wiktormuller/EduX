namespace Edux.Shared.Infrastructure.RabbitMQ.Contexts
{
    internal interface IMessageContextProvider
    {
        string HeaderName { get; }
        object Get(IDictionary<string, object> headers);
    }
}
