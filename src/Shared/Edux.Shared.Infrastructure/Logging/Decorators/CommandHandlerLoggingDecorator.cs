using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Logging;
using Edux.Shared.Infrastructure.Decorator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace Edux.Shared.Infrastructure.Logging.Decorators
{
    [Decorator]
    internal sealed class CommandHandlerLoggingDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly ILogger<CommandHandlerLoggingDecorator<TCommand>> _logger;
        private readonly ICommandHandler<TCommand> _commandHandler;
        private readonly IMessageToLogTemplateMapper _mapper;

        public CommandHandlerLoggingDecorator(ILogger<CommandHandlerLoggingDecorator<TCommand>> logger, 
            ICommandHandler<TCommand> commandHandler,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _commandHandler = commandHandler;
            _mapper = serviceProvider.GetService<IMessageToLogTemplateMapper>() 
                ?? new EmptyMessageToLogTemplateMapper();
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            var template = _mapper.Map(command);

            if (template is null)
            {
                await _commandHandler.HandleAsync(command, cancellationToken);
                return;
            }

            try
            {
                Log(command, template.Before);
                await _commandHandler.HandleAsync(command, cancellationToken);
                Log(command, template.After);
            }
            catch (Exception ex)
            {
                var exceptionTemplate = template.GetExceptionTemplate(ex);

                Log(command, exceptionTemplate, isError: true);
                throw;
            }
        }

        private void Log(TCommand command, string message, bool isError = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (isError)
            {
                _logger.LogError(Smart.Format(message, command));
            }
            else
            {
                _logger.LogInformation(Smart.Format(message, command));
            }
        }

        private class EmptyMessageToLogTemplateMapper : IMessageToLogTemplateMapper
        {
            public HandlerLogTemplate Map<TMessage>(TMessage message) where TMessage : class => null;
        }
    }
}
