namespace Edux.Shared.Infrastructure.Storage.Mongo.Context
{
    internal sealed class OperationsContext : IOperationsContext
    {
        private readonly List<Func<Task>> _operations;

        public IEnumerable<Func<Task>> Operations => _operations;

        public OperationsContext()
        {
            _operations = [];
        }

        public void AddOperation(Func<Task> operation)
        {
            _operations.Add(operation);
        }
    }
}
