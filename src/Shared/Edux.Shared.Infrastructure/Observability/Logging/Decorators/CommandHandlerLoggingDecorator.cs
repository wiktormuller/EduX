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
        private readonly ICorrelationContext _context;

        public CommandHandlerLoggingDecorator(ILogger<CommandHandlerLoggingDecorator<TCommand>> logger,
            ICommandHandler<TCommand> commandHandler,
            ICorrelationContext context)
        {
            _logger = logger;
            _commandHandler = commandHandler;
            _context = context;
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            // We can use template mechanism here with some global registry or implement simple loggin with predefined data

            var module = command.GetModuleName();
            var name = command.GetType().Name.Underscore();
            var requestId = _context.RequestId;
            var traceId = _context.TraceId;
            var userId = _context.Identity?.Id;
            // TODO: Implement MessageId and CorrelationId

            try
            {
                _logger.LogInformation($"Handling a command: {name} ({module}) " +
                    $"[Request ID: {requestId}, " +
                    $"Trace ID: '{traceId}', User ID: '{userId}]'...");

                await _commandHandler.HandleAsync(command, cancellationToken);

                _logger.LogInformation($"Handled a command: {name} ({module}) " +
                    $"[Request ID: {requestId}, " +
                    $"Trace ID: '{traceId}', User ID: '{userId}']");
            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an ERROR while handling a command: {name} ({module}) " +
                    $"[Request ID: {requestId}, " +
                    $"Trace ID: '{traceId}', User ID: '{userId}]'. Exception message: {ex.Message}");

                throw;
            }
        }
    }
}
