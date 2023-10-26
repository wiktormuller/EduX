using Edux.Shared.Abstractions.Messaging;

namespace Edux.Shared.Infrastructure.Messaging.Outbox.Registries
{
    internal sealed class OutboxTypeRegistry
    {
        private readonly Dictionary<string, Type> _types = new(); // Outbox per module

        public void Register<T>() where T : IMessageOutbox
            => _types[GetKey<T>()] = typeof(T);

        public Type Resolve(IMessage message)
            => _types.TryGetValue(GetKey(message.GetType()), out var type) ? type : null;

        private static string GetKey<T>()
            => GetKey(typeof(T));

        private static string GetKey(Type type)
            => type.IsGenericType
                ? $"{type.GenericTypeArguments[0].GetModuleName()}"
                : $"{type.GetModuleName()}";
    }
}
