namespace Edux.Shared.Abstractions.Contexts
{
    public interface ICorrelationContextAccessor
    {
        ICorrelationContext CorrelationContext { get; set; }
    }
}
