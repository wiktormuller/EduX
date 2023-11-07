namespace Edux.Shared.Infrastructure.Api.RateLimiting
{
    internal sealed class RateLimitOptions
    {
        public int PermitLimit { get; set; }
        public int WindowInSeconds { get; set; }
        public int QueueLimit { get; set; }
    }
}
