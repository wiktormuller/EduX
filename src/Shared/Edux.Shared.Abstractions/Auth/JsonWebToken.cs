namespace Edux.Shared.Abstractions.Auth
{
    public class JsonWebToken
    {
        public string AccessToken { get; }
        public string RefreshToken { get; private set; }
        public long Expires { get; }
        public string Id { get; }
        public string Role { get; }
        public string Email { get; }
        public IDictionary<string, IEnumerable<string>> Claims { get; }

        public JsonWebToken(string accessToken, string refreshToken, long expires, string id, string role, string email,
            IDictionary<string, IEnumerable<string>> claims)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Expires = expires;
            Id = id;
            Role = role;
            Email = email;
            Claims = claims;
        }

        public void SetRefreshToken(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
