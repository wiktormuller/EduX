namespace Edux.Shared.Abstractions.SharedKernel
{
    public interface IDomainEventHandler<TEvent> where TEvent : class, IDomainEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
