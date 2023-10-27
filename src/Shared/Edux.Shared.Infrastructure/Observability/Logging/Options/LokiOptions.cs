namespace Edux.Shared.Infrastructure.Observability.Logging.Options
{
    internal sealed class LokiOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        public int? BatchPostingLimit { get; set; }
        public int? QueueLimit { get; set; }
        public TimeSpan? Period { get; set; }
        public string LokiUsername { get; set; }
        public string LokiPassword { get; set; }
    }
}
