namespace Edux.Shared.Infrastructure.Modules.Registries
{
    internal interface IModuleRegistry
    {
        void AddRequestAction(string path, Type requestType, Type responseType,
            Func<object, CancellationToken, Task<object>> action);
        ModuleRequestRegistration GetRequestRegistration(string path);

        void AddBroadcastAction(Type requestType, Func<object, Task> action);
        IEnumerable<ModuleBroadcastRegistration> GetBroadcastRegistrations(string key);
    }
}
