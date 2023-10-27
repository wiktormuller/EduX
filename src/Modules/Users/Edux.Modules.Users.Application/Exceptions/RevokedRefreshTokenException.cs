namespace Edux.Modules.Users.Application.Exceptions
{
    internal class RevokedRefreshTokenException : Shared.Abstractions.SharedKernel.Exceptions.ApplicationException
    {
        public override string Code { get; } = "revoked_refresh_token";

        public RevokedRefreshTokenException() : base("Revoked refresh token.")
        {
        }
    }
}
