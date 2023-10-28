namespace Edux.Shared.Abstractions.Messaging.Contexts
{
    public interface IMessageContextProvider
    {
        IMessageContext GetCurrent();
        void Set(IMessageContext messageContext);
    }
}
