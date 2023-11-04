using Edux.Shared.Abstractions.Contexts;

namespace Edux.Shared.Infrastructure.HTTP.Middlewares
{
    internal sealed class CorrelationIdMessageHandler : DelegatingHandler
    {
        private const string CorrelationIdKey = "correlation-id";
        private readonly IContextProvider _contextProvider;

        public CorrelationIdMessageHandler(IContextProvider contextProvider)
        {
            _contextProvider = contextProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _contextProvider.Current();
            var correlationId = context.CorrelationId == Guid.Empty
                ? Guid.NewGuid().ToString("N")
                : context.CorrelationId.ToString("N");

            request.Headers.TryAddWithoutValidation(CorrelationIdKey, correlationId);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
