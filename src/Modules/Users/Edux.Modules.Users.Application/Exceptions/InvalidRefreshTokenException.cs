using Edux.Shared.Abstractions.Kernel.Exceptions;

namespace Edux.Modules.Users.Application.Exceptions
{
    internal class InvalidRefreshTokenException : Shared.Abstractions.Kernel.Exceptions.ApplicationException
    {
        public override string Code { get; } = "invalid_refresh_token";

        public InvalidRefreshTokenException() : base("Invalid refresh token.")
        {
        }
    }
}
