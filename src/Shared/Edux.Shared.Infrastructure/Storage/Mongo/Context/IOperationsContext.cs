namespace Edux.Shared.Infrastructure.Storage.Mongo.Context
{
    public interface IOperationsContext
    {
        IEnumerable<Func<Task>> Operations { get; }
        void AddOperation(Func<Task> operation);
    }
}
