using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Infrastructure.Decorator;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Observability.Logging.Decorators
{
    [Decorator]
    internal sealed class DomainEventHandlerLoggingDecorator<TEvent> : IDomainEventHandler<TEvent>
        where TEvent : class, IDomainEvent
    {
        private readonly ILogger<DomainEventHandlerLoggingDecorator<TEvent>> _logger;
        private readonly IDomainEventHandler<TEvent> _domainEventHandler;
        private readonly IContextProvider _contextProvider;

        public DomainEventHandlerLoggingDecorator(ILogger<DomainEventHandlerLoggingDecorator<TEvent>> logger,
            IDomainEventHandler<TEvent> domainEventHandler,
            IContextProvider contextProvider)
        {
            _logger = logger;
            _domainEventHandler = domainEventHandler;
            _contextProvider = contextProvider;
        }

        public async Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken)
        {
            // We can use template mechanism here with some global registry or implement simple loggin with predefined data

            var context = _contextProvider.Current();
            var module = domainEvent.GetModuleName();
            var name = domainEvent.GetType().Name.Underscore();
            var requestId = context?.RequestContext?.RequestId;
            var traceId = context?.TraceId;
            var userId = context?.IdentityContext?.Id;
            var correlationId = context?.CorrelationId;
            var messageId = context?.MessageContext?.MessageId;

            try
            {
                _logger.LogInformation($"Handling a domain event: {name} ({module}) " +
                    $"[Request ID: {requestId}, Message ID: '{messageId}', " +
                $"Trace ID: '{traceId}', Correlation ID: '{correlationId}', User ID: '{userId}]'...");

                await _domainEventHandler.HandleAsync(domainEvent, cancellationToken);

                _logger.LogInformation($"Handled a domain event: {name} ({module}) " +
                    $"[Request ID: {requestId}, Message ID: '{messageId}', " +
                    $"Trace ID: '{traceId}', Correlation ID: '{correlationId}', User ID: '{userId}']");
            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an ERROR while handling a domain event: {name} ({module}) " +
                    $"[Request ID: {requestId}, Message ID: '{messageId}', " +
                    $"Trace ID: '{traceId}', Correlation ID: '{correlationId}', User ID: '{userId}]'. " +
                    $"Exception message: {ex.Message}");

                throw;
            }
        }
    }
}
