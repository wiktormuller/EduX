using Edux.Shared.Abstractions.Exceptions;

namespace Edux.Shared.Infrastructure.Exceptions
{
    internal interface IExceptionDispatcher
    {
        ExceptionResponse Handle(Exception exception);
    }
}
