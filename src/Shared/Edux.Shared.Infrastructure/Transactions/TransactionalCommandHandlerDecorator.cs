using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.SqlServer;
using Edux.Shared.Infrastructure.Decorator;
using Edux.Shared.Infrastructure.SqlServer;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Transactions
{
    [Decorator]
    internal sealed class TransactionalCommandHandlerDecorator<T> : ICommandHandler<T> where T : class, ICommand
    {
        private readonly ICommandHandler<T> _commandHandler;
        private readonly UnitOfWorkTypeRegistry _unitOfWorkTypeRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransactionalCommandHandlerDecorator<T>> _logger;

        public TransactionalCommandHandlerDecorator(ICommandHandler<T> commandHandler,
            UnitOfWorkTypeRegistry unitOfWorkTypeRegistry,
            IServiceProvider serviceProvider,
            ILogger<TransactionalCommandHandlerDecorator<T>> logger)
        {
            _commandHandler = commandHandler;
            _unitOfWorkTypeRegistry = unitOfWorkTypeRegistry;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task HandleAsync(T command, CancellationToken cancellationToken)
        {
            var unitOfWorkType = _unitOfWorkTypeRegistry.Resolve<T>();
            if (unitOfWorkType is null)
            {
                await _commandHandler.HandleAsync(command, cancellationToken);
                return;
            }

            var unitOfWork = (IUnitOfWork)_serviceProvider.GetRequiredService(unitOfWorkType);
            var unitOfWorkName = unitOfWorkType.Name;
            var name = command.GetType().Name.Underscore();

            _logger.LogInformation("Handling: {Name} using Transaction ({UnitOfWorkName})...", name, unitOfWorkName);

            await unitOfWork.ExecuteAsync(() 
                => _commandHandler.HandleAsync(command, cancellationToken), cancellationToken);

            _logger.LogInformation("Handled: {Name} using TX ({UnitOfWorkName})", name, unitOfWorkName);
        }
    }
}
