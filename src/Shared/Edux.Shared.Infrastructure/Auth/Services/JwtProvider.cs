using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Auth.Options;
using Edux.Shared.Infrastructure.Time;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Edux.Shared.Infrastructure.Auth.Services
{
    internal sealed class JwtProvider : IJwtProvider
    {
        private static readonly IDictionary<string, IEnumerable<string>> EmptyClaims =
            new Dictionary<string, IEnumerable<string>>();

        private static readonly ISet<string> DefaultClaims = new HashSet<string>
        {
            JwtRegisteredClaimNames.Sub,
            JwtRegisteredClaimNames.UniqueName,
            JwtRegisteredClaimNames.Jti,
            JwtRegisteredClaimNames.Iat,
            ClaimTypes.Role,
        };

        private readonly IClock _clock;
        private readonly AuthOptions _authOptions;
        private readonly SigningCredentials _signingCredentials;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtProvider(IClock clock, 
            AuthOptions authOptions, 
            TokenValidationParameters tokenValidationParameters)
        {
            if (authOptions.IssuerSigningKey is null)
            {
                throw new InvalidOperationException("Issuer signing key is not set.");
            }

            _signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.IssuerSigningKey)),
                SecurityAlgorithms.HmacSha256);

            _clock = clock;
            _authOptions = authOptions;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public JsonWebToken CreateToken(string userId, string email, string role, string? audience = null,
            IDictionary<string, IEnumerable<string>>? claims = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID claim (subject) cannot be empty.", nameof(userId));
            }

            var now = _clock.CurrentDate();

            var jwtClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.UniqueName, userId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeStamp().ToString())
            };

            if (!string.IsNullOrWhiteSpace(role))
            {
                jwtClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (!string.IsNullOrWhiteSpace(audience))
            {
                jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
            }

            if (claims?.Any() is true)
            {
                var customClaims = new List<Claim>();
                foreach (var (claim, values) in claims)
                {
                    customClaims.AddRange(values.Select(value => new Claim(claim, value)));
                }

                jwtClaims.AddRange(customClaims);
            }

            var expires = now.Add(_authOptions.Expiry); // In minutes

            var jwt = new JwtSecurityToken(
                _authOptions.Issuer,
                claims: jwtClaims,
                notBefore: now,
                expires: expires,
                signingCredentials: _signingCredentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new JsonWebToken
            (
                accessToken,
                string.Empty,
                expires.ToUnixTimeStamp(),
                userId,
                role ?? string.Empty,
                email,
                claims ?? EmptyClaims
            );
        }

        public JsonWebTokenPayload? GetTokenPayload(string accessToken)
        {
            _jwtSecurityTokenHandler.ValidateToken(accessToken, _tokenValidationParameters, out var validatedSecurityToken);

            if (validatedSecurityToken is not JwtSecurityToken jwt)
            {
                return null;
            }

            return new JsonWebTokenPayload
            (
                jwt.Subject,
                jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value,
                jwt.ValidTo.ToUnixTimeStamp(),
                jwt.Claims.Where(c => !DefaultClaims.Contains(c.Type))
                    .GroupBy(c => c.Type)
                    .ToDictionary(k => k.Key, v => v.Select(c => c.Value))
            );
        }
    }
}
