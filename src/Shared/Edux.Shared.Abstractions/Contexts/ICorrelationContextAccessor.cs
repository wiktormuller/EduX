namespace Edux.Shared.Abstractions.Contexts
{
    public interface ICorrelationContextAccessor
    {
        object CorrelationContext { get; set; }
    }
}
