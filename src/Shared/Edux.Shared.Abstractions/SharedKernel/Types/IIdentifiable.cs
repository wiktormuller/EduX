namespace Edux.Shared.Abstractions.SharedKernel.Types
{
    public interface IIdentifiable<out T>
    {
        T Id { get; }
    }
}
