namespace Edux.Shared.Abstractions.Auth
{
    public interface IRefreshTokenProvider
    {
        Task<string> CreateAsync(Guid userId);
        Task RevokeAsync(string refreshToken);
        Task<JsonWebToken> UseAsync(string refreshToken);
    }
}
