namespace Edux.Shared.Abstractions.SharedKernel
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(params IDomainEvent[] events);
    }
}
