namespace Edux.Shared.Abstractions.Events
{
    public interface IEventDispatcher
    {
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class, IEvent;
    }
}
