using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Shared.Infrastructure.Messaging.Inbox.Registries
{
    internal sealed class InboxTypeRegistry
    {
        private readonly Dictionary<string, Type> _types = new(); // Inbox per module

        public void Register<T>() where T : IMessageInbox
            => _types[GetKey<T>()] = typeof(T);

        public Type? Resolve(IMessage message)
            => _types.TryGetValue(GetKey(message.GetType()), out var type) ? type : null;

        public Type? Resolve(ICommand command)
            => _types.TryGetValue(GetKey(command.GetType()), out var type) ? type : null;

        private static string GetKey<T>()
            => GetKey(typeof(T));

        private static string GetKey(Type type)
            => type.IsGenericType
                ? $"{type.GenericTypeArguments[0].GetModuleName()}"
                : $"{type.GetModuleName()}";
    }
}
