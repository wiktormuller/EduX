namespace Edux.Shared.Infrastructure.Modules.Registries
{
    internal sealed class ModuleBroadcastRegistration
    {
        public Type ReceiverType { get; }
        public Func<object, Task> Action { get; }
        public string Key => ReceiverType.Name;

        public ModuleBroadcastRegistration(Type receiverType, Func<object, Task> action)
        {
            ReceiverType = receiverType;
            Action = action;
        }

    }
}
