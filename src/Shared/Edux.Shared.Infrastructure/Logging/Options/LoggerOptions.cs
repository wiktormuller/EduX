namespace Edux.Shared.Infrastructure.Logging.Options
{
    internal sealed class LoggerOptions
    {
        public string Level { get; set; } = string.Empty;
        public ConsoleOptions Console { get; set; } = new();
        public FileOptions File { get; set; } = new();
        public ElkOptions Elk { get; set; } = new();
        public LokiOptions Loki { get; set; } = new();
        public SeqOptions Seq { get; set; } = new();
        public Dictionary<string, string> MinimumLevelOverrides { get; set; } = new();
        public IEnumerable<string>? ExcludePaths { get; set; }
        public IEnumerable<string>? ExcludeProperties { get; set; }
        public Dictionary<string, object> Tags { get; set; } = new();
    }
}
