namespace Edux.Shared.Abstractions.Contexts
{
    public interface IContextAccessor
    {
        IContext? Context { get; set; }
    }
}
