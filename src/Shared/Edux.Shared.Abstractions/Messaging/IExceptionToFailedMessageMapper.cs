namespace Edux.Shared.Abstraction.Messaging
{
    public interface IExceptionToFailedMessageMapper
    {
        FailedMessage Map(Exception exception, object message);
    }
}
