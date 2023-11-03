namespace Edux.Shared.Abstractions.Contexts
{
    public interface IRequestContext
    {
        Guid RequestId { get; }
        string IpAddress { get; }
        string UserAgent { get; }
    }
}
