using Edux.Shared.Abstractions.Auth;
using Microsoft.AspNetCore.Http;

namespace Edux.Shared.Infrastructure.Auth.Services
{
    internal sealed class HttpContextTokenStorage : ITokenStorage
    {
        private const string TokenKey = "jwt";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextTokenStorage(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public JsonWebToken? Get()
        {
            if (_httpContextAccessor.HttpContext is null)
            {
                return null;
            }

            if (_httpContextAccessor.HttpContext.Items.TryGetValue(TokenKey, out var jwt))
            {
                return jwt as JsonWebToken;
            }

            return null;
        }

        public void Set(JsonWebToken jwt)
        {
            _httpContextAccessor.HttpContext?.Items.TryAdd(TokenKey, jwt);
        }
    }
}
