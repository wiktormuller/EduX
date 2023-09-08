using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Infrastructure.Decorator;
using Edux.Shared.Infrastructure.SqlServer;

namespace Edux.Shared.Infrastructure.Transactions
{
    [Decorator]
    internal sealed class TransactionalCommandHandlerDecorator<T> : ICommandHandler<T> where T : class, ICommand
    {
        private readonly ICommandHandler<T> _commandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionalCommandHandlerDecorator(ICommandHandler<T> commandHandler, 
            IUnitOfWork unitOfWork)
        {
            _commandHandler = commandHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(T command, CancellationToken cancellationToken)
        {
            await _unitOfWork.ExecuteAsync(() 
                => _commandHandler.HandleAsync(command, cancellationToken), cancellationToken);
        }
    }
}
