namespace Edux.Modules.Users.Application.Contracts.Requests
{
    public class RevokeRefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }
}
