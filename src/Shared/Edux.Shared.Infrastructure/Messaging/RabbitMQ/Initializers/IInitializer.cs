namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Initializers
{
    internal interface IInitializer
    {
        Task InitializeAsync();
    }
}
