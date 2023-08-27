namespace Edux.Shared.Abstractions.Commands
{
    public interface ICommandDispatcher
    {
        public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : class, ICommand;
    }
}
