namespace Edux.Shared.Infrastructure.Initializers
{
    internal interface IStartupInitializer : IInitializer
    {
        void AddInitializer(IInitializer initializer);
    }
}
