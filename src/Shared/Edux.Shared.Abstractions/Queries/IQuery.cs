namespace Edux.Shared.Abstractions.Queries
{
    // Marker
    public interface IQuery
    {
    }

    public interface IQuery<TResult> : IQuery
    { 
    }
}
