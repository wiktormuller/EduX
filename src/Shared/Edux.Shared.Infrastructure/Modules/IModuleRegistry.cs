namespace Edux.Shared.Infrastructure.Modules
{
    internal interface IModuleRegistry
    {
        void AddRequestAction(string path, Type requestType, Type responseType, 
            Func<object, CancellationToken, Task<object>> action);

        ModuleRequestRegistration GetRequestRegistration(string path);
    }
}
