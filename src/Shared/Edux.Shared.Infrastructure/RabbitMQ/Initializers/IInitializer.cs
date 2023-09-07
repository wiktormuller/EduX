namespace Edux.Shared.Infrastructure.RabbitMQ.Initializers
{
    internal interface IInitializer
    {
        Task InitializeAsync();
    }
}
