﻿namespace Edux.Shared.Infrastructure.Logging.Options
{
    internal sealed class LoggerOptions
    {
        public string Level { get; set; }
        public ConsoleOptions Console { get; set; }
        public FileOptions File { get; set; }
        public ElkOptions Elk { get; set; }
        public SeqOptions Seq { get; set; }
        public LokiOptions Loki { get; set; }
        public IDictionary<string, string> MinimumLevelOverrides { get; set; }
        public IEnumerable<string> ExcludePaths { get; set; }
        public IEnumerable<string> ExcludeProperties { get; set; }
        public IDictionary<string, object> Tags { get; set; }
    }
}
