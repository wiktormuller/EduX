namespace Edux.Shared.Infrastructure.Auth.Services
{
    internal interface IApiKeyValidator
    {
        bool IsApiKeyValid(string userApiKey);
    }
}
