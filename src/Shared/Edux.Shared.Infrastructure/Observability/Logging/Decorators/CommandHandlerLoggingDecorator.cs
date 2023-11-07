using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Infrastructure.Decorator;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Observability.Logging.Decorators
{
    [Decorator]
    internal sealed class CommandHandlerLoggingDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly ILogger<CommandHandlerLoggingDecorator<TCommand>> _logger;
        private readonly ICommandHandler<TCommand> _commandHandler;
        private readonly IContextProvider _contextProvider;

        public CommandHandlerLoggingDecorator(ILogger<CommandHandlerLoggingDecorator<TCommand>> logger,
            ICommandHandler<TCommand> commandHandler,
            IContextProvider contextProvider)
        {
            _logger = logger;
            _commandHandler = commandHandler;
            _contextProvider = contextProvider;
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            // We can use template mechanism here with some global registry or implement simple loggin with predefined data

            var context = _contextProvider.Current();
            var module = command.GetModuleName();
            var name = command.GetType().Name.Underscore();
            var requestId = context?.RequestContext.RequestId;
            var traceId = context.TraceId;
            var userId = context?.IdentityContext?.Id;
            var correlationId = context?.CorrelationId;
            var messageId = context?.MessageContext?.MessageId;

            try
            {
                _logger.LogInformation($"Handling a command: {name} ({module}) " +
                    $"[Request ID: {requestId}, Message ID: '{messageId}', " +
                    $"Trace ID: '{traceId}', Correlation ID: '{correlationId}', User ID: '{userId}]'...");

                await _commandHandler.HandleAsync(command, cancellationToken);

                _logger.LogInformation($"Handled a command: {name} ({module}) " +
                    $"[Request ID: {requestId}, Message ID: '{messageId}', " +
                    $"Trace ID: '{traceId}', Correlation ID: '{correlationId}', User ID: '{userId}']");
            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an ERROR while handling a command: {name} ({module}) " +
                    $"[Request ID: {requestId}, Message ID: '{messageId}', " +
                    $"Trace ID: '{traceId}', Correlation ID: '{correlationId}', User ID: '{userId}]'. " +
                    $"Exception message: {ex.Message}");

                throw;
            }
        }
    }
}
