using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Abstractions.Transactions;
using Edux.Shared.Infrastructure.Decorator;
using Edux.Shared.Infrastructure.Transactions.Registries;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Edux.Shared.Infrastructure.Transactions.Decorators
{
    [Decorator]
    internal sealed class TransactionalDomainEventHandlerDecorator<T> : IDomainEventHandler<T> where T : class, IDomainEvent
    {
        private readonly IDomainEventHandler<T> _domainEventHandler;
        private readonly UnitOfWorkTypeRegistry _unitOfWorkTypeRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransactionalDomainEventHandlerDecorator<T>> _logger;

        public TransactionalDomainEventHandlerDecorator(IDomainEventHandler<T> domainEventHandler,
            UnitOfWorkTypeRegistry unitOfWorkTypeRegistry,
            IServiceProvider serviceProvider,
            ILogger<TransactionalDomainEventHandlerDecorator<T>> logger)
        {
            _domainEventHandler = domainEventHandler;
            _unitOfWorkTypeRegistry = unitOfWorkTypeRegistry;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task HandleAsync(T domainEvent, CancellationToken cancellationToken)
        {
            var unitOfWorkType = _unitOfWorkTypeRegistry.Resolve<T>();
            if (unitOfWorkType is null)
            {
                await _domainEventHandler.HandleAsync(domainEvent, cancellationToken);
                return;
            }

            var unitOfWork = (IUnitOfWork)_serviceProvider.GetRequiredService(unitOfWorkType);
            var unitOfWorkName = unitOfWorkType.Name;
            var name = domainEvent.GetType().Name.Underscore();

            _logger.LogInformation("Handling: {Name} using Transaction ({UnitOfWorkName})...", name, unitOfWorkName);

            await unitOfWork.ExecuteAsync(()
                => _domainEventHandler.HandleAsync(domainEvent, cancellationToken), cancellationToken);

            _logger.LogInformation("Handled: {Name} using TX ({UnitOfWorkName})", name, unitOfWorkName);
        }
    }
}
