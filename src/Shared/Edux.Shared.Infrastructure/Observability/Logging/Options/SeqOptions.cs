namespace Edux.Shared.Infrastructure.Observability.Logging.Options
{
    internal sealed class SeqOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
