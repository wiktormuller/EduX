namespace Edux.Shared.Infrastructure.RabbitMQ.Initializers
{
    internal interface IStartupInitializer : IInitializer
    {
        void AddInitializer(IInitializer initializer);
    }
}
