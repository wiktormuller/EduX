namespace Edux.Shared.Abstraction.Messaging
{
    public interface IExceptionToMessageMapper
    {
        object Map(Exception exception, object message);
    }
}
