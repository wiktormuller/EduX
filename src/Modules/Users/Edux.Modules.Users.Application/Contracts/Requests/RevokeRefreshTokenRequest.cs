namespace Edux.Modules.Users.Application.Contracts.Requests
{
    public class RevokeRefreshTokenRequest // TODO: Add validation
    {
        public string RefreshToken { get; set; }
    }
}
