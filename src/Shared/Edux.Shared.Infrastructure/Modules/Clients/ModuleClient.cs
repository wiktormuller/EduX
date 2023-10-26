using Edux.Shared.Abstractions.Modules;
using Edux.Shared.Infrastructure.Modules.Registries;
using Edux.Shared.Infrastructure.Modules.Serializers;

namespace Edux.Shared.Infrastructure.Modules.Clients
{
    internal sealed class ModuleClient : IModuleClient
    {
        private readonly IModuleRegistry _moduleRegistry;
        private readonly IModuleSerializer _moduleSerializer;

        public ModuleClient(IModuleRegistry moduleRegistry,
            IModuleSerializer moduleSerializer)
        {
            _moduleRegistry = moduleRegistry;
            _moduleSerializer = moduleSerializer;
        }

        public async Task PublishAsync(object message)
        {
            var key = message.GetType().Name;
            var registrations = _moduleRegistry.GetBroadcastRegistrations(key);

            var tasks = new List<Task>();

            foreach (var registration in registrations)
            {
                var translatedMessageForReceiver = TranslateType(message, registration.ReceiverType);
                var action = registration.Action;
                tasks.Add(action(translatedMessageForReceiver));
            }

            Task.WhenAll(tasks); // It's fully synchronous operation
        }

        public Task SendAsync(string path, object request, CancellationToken cancellationToken = default)
        {
            return SendAsync<object>(path, request, cancellationToken);
        }

        public async Task<TResult> SendAsync<TResult>(string path, object request, CancellationToken cancellationToken = default) where TResult : class
        {
            var registration = _moduleRegistry.GetRequestRegistration(path);

            if (registration is null)
            {
                throw new InvalidOperationException($"No action has been defined for path: '{path}'.");
            }

            var receiverRequest = TranslateType(request, registration.RequestType);
            var result = await registration.Action(receiverRequest, cancellationToken);

            return result is null
                ? null
                : TranslateType<TResult>(result);
        }

        private T TranslateType<T>(object value)
        {
            return _moduleSerializer.Deserialize<T>(_moduleSerializer.Serialize(value));
        }

        private object TranslateType(object value, Type type)
        {
            return _moduleSerializer.Deserialize(_moduleSerializer.Serialize(value), type);
        }
    }
}
