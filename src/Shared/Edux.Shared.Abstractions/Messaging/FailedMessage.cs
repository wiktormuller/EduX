namespace Edux.Shared.Abstraction.Messaging
{
    public sealed class FailedMessage
    {
        public object Message { get; }
        public bool ShouldRetry { get; }
        public bool MoveToDeadLetter { get; }

        public FailedMessage(bool shouldRetry = true, bool moveToDeadLetter = true) : this(null, shouldRetry, moveToDeadLetter)
        {
        }

        public FailedMessage(object message, bool shouldRetry = true, bool moveToDeadLetter = true)
        {
            Message = message;
            ShouldRetry = shouldRetry;
            MoveToDeadLetter = moveToDeadLetter;
        }
    }
}
