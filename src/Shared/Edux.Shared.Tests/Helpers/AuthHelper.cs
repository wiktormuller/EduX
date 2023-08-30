using Edux.Shared.Infrastructure.Auth;
using Edux.Shared.Infrastructure.Time;

namespace Edux.Shared.Tests.Helpers
{
    public static class AuthHelper
    {
        private static readonly JwtProvider _jwtProvider;

        static AuthHelper()
        {
            var options = OptionsHelper.GetOptions<AuthOptions>("auth");
            _jwtProvider = new JwtProvider(new UtcClock(), options);
        }

        public static string CreateJwtToken(Guid userId, string email, string role = null, string audience = null,
            IDictionary<string, IEnumerable<string>> claims = null)
        {
            return _jwtProvider.CreateToken(userId.ToString(), email, role, audience, claims)
                .AccessToken;
        }
    }
}
