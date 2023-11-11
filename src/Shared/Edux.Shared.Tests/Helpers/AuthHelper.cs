using Edux.Shared.Infrastructure.Auth.Options;
using Edux.Shared.Infrastructure.Auth.Services;
using Edux.Shared.Infrastructure.Time;
using Microsoft.IdentityModel.Tokens;

namespace Edux.Shared.Tests.Helpers
{
    public static class AuthHelper
    {
        private static readonly JwtProvider _jwtProvider;

        static AuthHelper()
        {
            var options = OptionsHelper.GetOptions<AuthOptions>("auth");
            _jwtProvider = new JwtProvider(new UtcClock(), options, new TokenValidationParameters());
        }

        public static string CreateJwtToken(Guid userId, string email, string role, string? audience = null,
            IDictionary<string, IEnumerable<string>>? claims = null)
        {
            return _jwtProvider.CreateToken(userId.ToString(), email, role, audience, claims)
                .AccessToken;
        }
    }
}
