namespace Edux.Shared.Abstractions.Kernel
{
    public interface IDomainEventHandler<TEvent> where TEvent : class, IDomainEvent
    {
        Task HandleAsync(TEvent @event);
    }
}
