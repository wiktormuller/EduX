namespace Edux.Shared.Abstractions.Events
{
    public interface IEventDispatcher
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;
    }
}
