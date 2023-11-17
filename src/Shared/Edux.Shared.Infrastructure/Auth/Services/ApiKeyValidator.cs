using Edux.Shared.Infrastructure.Auth.Options;

namespace Edux.Shared.Infrastructure.Auth.Services
{
    internal sealed class ApiKeyValidator : IApiKeyValidator
    {
        private readonly AuthOptions _options;

        public ApiKeyValidator(AuthOptions options)
        {
            _options = options;
        }

        public bool IsApiKeyValid(string userApiKey)
        {
            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                return false;
            }

            var apiKey = _options.ApiKey;

            if (apiKey is null || apiKey != userApiKey)
            {
                return false;
            }

            return true;
        }
    }
}
