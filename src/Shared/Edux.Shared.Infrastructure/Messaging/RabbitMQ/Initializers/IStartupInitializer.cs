namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Initializers
{
    internal interface IStartupInitializer : IInitializer
    {
        void AddInitializer(IInitializer initializer);
    }
}
