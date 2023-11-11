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
                var translatedMessageForReceiver = TranslateType(message, registration.ReceiverType)
                    ?? throw new InvalidOperationException($"Invalid translation of types between modules while calling Publish in ModuleClient");

                var action = registration.Action;
                tasks.Add(action(translatedMessageForReceiver));
            }

            await Task.WhenAll(tasks); // It's fully synchronous operation (in the same process)
        }

        public Task SendAsync(string path, object request, CancellationToken cancellationToken = default)
        {
            return SendAsync<object>(path, request, cancellationToken);
        }

        public async Task<TResult?> SendAsync<TResult>(string path, object request, CancellationToken cancellationToken = default) where TResult : class
        {
            var registration = _moduleRegistry.GetRequestRegistration(path)
                ?? throw new InvalidOperationException($"No action has been defined for path: '{path}'."); ;

            var receiverRequest = TranslateType(request, registration.RequestType)
                ?? throw new InvalidOperationException($"Invalid translation of types between modules while calling Send in ModuleClient");

            var result = await registration.Action(receiverRequest, cancellationToken);

            return result is null
                ? null
                : TranslateType<TResult>(result);
        }

        private T? TranslateType<T>(object value)
        {
            return _moduleSerializer.Deserialize<T>(_moduleSerializer.Serialize(value));
        }

        private object? TranslateType(object value, Type type)
        {
            return _moduleSerializer.Deserialize(_moduleSerializer.Serialize(value), type);
        }
    }
}
