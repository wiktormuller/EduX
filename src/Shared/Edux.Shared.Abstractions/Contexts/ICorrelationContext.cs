namespace Edux.Shared.Abstractions.Contexts
{
    public interface ICorrelationContext
    {
        Guid RequestId { get; }
        Guid CorrelationId { get; }
        string TraceId { get; }
        string IpAddress { get; }
        string UserAgent { get; }
        IIdentityContext Identity { get; }
    }
}
