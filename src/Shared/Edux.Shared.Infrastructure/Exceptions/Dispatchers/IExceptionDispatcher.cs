using Edux.Shared.Abstractions.Exceptions;

namespace Edux.Shared.Infrastructure.Exceptions.Dispatchers
{
    internal interface IExceptionDispatcher
    {
        ExceptionResponse Handle(Exception exception);
    }
}
