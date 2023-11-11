namespace Edux.Shared.Infrastructure.Modules.Registries
{
    internal sealed class ModuleRegistry : IModuleRegistry
    {
        private readonly List<ModuleBroadcastRegistration> _broadcastRegistrations = new();
        private readonly Dictionary<string, ModuleRequestRegistration> _requestRegistrations = new();

        public void AddBroadcastAction(Type requestType, Func<object, Task> action)
        {
            if (string.IsNullOrWhiteSpace(requestType.Namespace))
            {
                throw new InvalidOperationException("Missing namespace.");
            }

            var registration = new ModuleBroadcastRegistration(requestType, action);
            _broadcastRegistrations.Add(registration);
        }

        public IEnumerable<ModuleBroadcastRegistration> GetBroadcastRegistrations(string key)
        {
            return _broadcastRegistrations.Where(x => x.Key == key);
        }

        public void AddRequestAction(string path, Type requestType, Type responseType,
            Func<object, CancellationToken, Task<object?>> action)
        {
            if (path is null)
            {
                throw new InvalidOperationException("Request path cannot be null.");
            }

            var registration = new ModuleRequestRegistration(requestType, responseType, action);
            _requestRegistrations.Add(path, registration);
        }

        public ModuleRequestRegistration? GetRequestRegistration(string path)
        {
            return _requestRegistrations.TryGetValue(path, out var registration)
                ? registration
                : null;
        }
    }
}
