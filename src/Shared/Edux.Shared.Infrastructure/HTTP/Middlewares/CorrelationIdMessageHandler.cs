using Edux.Shared.Abstractions.Contexts;

namespace Edux.Shared.Infrastructure.HTTP.Middlewares
{
    internal sealed class CorrelationIdMessageHandler : DelegatingHandler
    {
        private const string CorrelationIdKey = "correlation-id";
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdMessageHandler(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = _correlationContextAccessor.CorrelationContext.CorrelationId == Guid.Empty
                ? Guid.NewGuid().ToString("N")
                : _correlationContextAccessor.CorrelationContext.CorrelationId.ToString("N");

            request.Headers.TryAddWithoutValidation(CorrelationIdKey, correlationId);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
