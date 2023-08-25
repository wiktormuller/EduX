using Edux.Shared.Abstractions.Kernel.Exceptions;

namespace Edux.Modules.Users.Core.Exceptions
{
    public class EmptyRefreshTokenException : DomainException
    {
        public override string Code { get; } = "empty_refresh_token";

        public EmptyRefreshTokenException() : base("Empty refresh token.")
        {
        }
    }
}
