namespace Edux.Shared.Abstractions.Contexts
{
    public interface IContext
    {
        Guid CorrelationId { get; }
        string TraceId { get; }
        IRequestContext? RequestContext { get; }
        IIdentityContext? IdentityContext { get; }
        IMessageContext? MessageContext { get; }

        void SetMessageContext(IMessageContext messageContext);
    }
}
