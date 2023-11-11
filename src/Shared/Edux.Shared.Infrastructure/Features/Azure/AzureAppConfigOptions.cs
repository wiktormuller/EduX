namespace Edux.Shared.Infrastructure.Features.Azure
{
    public sealed class AzureAppConfigOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int CacheExpirationInterval { get; set; }
    }
}
