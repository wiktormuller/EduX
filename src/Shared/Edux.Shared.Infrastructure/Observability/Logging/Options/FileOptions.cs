namespace Edux.Shared.Infrastructure.Observability.Logging.Options
{
    internal sealed class FileOptions
    {
        public bool Enabled { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Interval { get; set; } = string.Empty;
    }
}
