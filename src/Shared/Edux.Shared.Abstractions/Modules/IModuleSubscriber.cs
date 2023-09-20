namespace Edux.Shared.Abstractions.Modules
{
    public interface IModuleSubscriber
    {
        IModuleSubscriber RegisterAction<TRequest, TResponse>(string path, Func<TRequest, IServiceProvider,
            CancellationToken, Task<TResponse>> action)
                where TRequest : class where TResponse : class;
    }
}
