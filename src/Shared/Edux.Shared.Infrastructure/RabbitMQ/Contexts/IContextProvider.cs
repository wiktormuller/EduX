namespace Edux.Shared.Infrastructure.RabbitMQ.Contexts
{
    internal interface IContextProvider
    {
        string HeaderName { get; }
        object Get(IDictionary<string, object> headers);
    }
}
