namespace Edux.Shared.Infrastructure.Modules
{
    internal sealed class ModuleRegistry : IModuleRegistry
    {
        private readonly Dictionary<string, ModuleRequestRegistration> _requestRegistrations = new();

        public void AddRequestAction(string path, Type requestType, Type responseType, 
            Func<object, CancellationToken, Task<object>> action)
        {
            if (path is null)
            {
                throw new InvalidOperationException("Request path cannot be null.");
            }

            var registration = new ModuleRequestRegistration(requestType, responseType, action);
            _requestRegistrations.Add(path, registration);
        }

        public ModuleRequestRegistration GetRequestRegistration(string path)
        {
            return _requestRegistrations.TryGetValue(path, out var registration)
                ? registration
                : null;
        }
    }
}
