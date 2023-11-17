using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Edux.Shared.Infrastructure.Auth.Services
{
    internal sealed class ApiKeyAuthorizationHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiKeyValidator _apiKeyValidator;

        public const string ApiKeyHeaderName = "X-API-Key";

        public ApiKeyAuthorizationHandler(IHttpContextAccessor httpContextAccessor,
            IApiKeyValidator apiKeyValidator)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiKeyValidator = apiKeyValidator;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            var apiKey = _httpContextAccessor?.HttpContext?.Request.Headers[ApiKeyHeaderName].ToString();
        
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (!_apiKeyValidator.IsApiKeyValid(apiKey))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
