using Edux.Shared.Abstractions.Contexts;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal class RequestContext : IRequestContext
    {
        public Guid RequestId { get; }

        public string IpAddress { get; }

        public string UserAgent { get; }

        public RequestContext(Guid requestId, string ipAddress, string userAgent)
        {
            RequestId = requestId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }
}
