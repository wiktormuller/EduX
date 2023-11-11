namespace Edux.Shared.Infrastructure.Observability.Logging.Options
{
    internal sealed class ElkOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool BasicAuthEnabled { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IndexFormat { get; set; } = string.Empty;
    }
}
