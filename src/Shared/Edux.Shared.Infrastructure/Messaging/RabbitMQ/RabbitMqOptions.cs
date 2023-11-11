namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ
{
    internal class RabbitMqOptions
    {
        public string ConnectionName { get; set; } = string.Empty;
        public IEnumerable<string> HostNames { get; set; } = new List<string>();
        public int Port { get; set; }
        public string VirtualHost { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan RequestedConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan SocketReadTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan SocketWriteTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan ContinuationTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public TimeSpan HandshakeContinuationTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan? MessageProcessingTimeout { get; set; }
        public ushort RequestedChannelMax { get; set; }
        public uint RequestedFrameMax { get; set; }
        public bool UseBackgroundThreadsForIO { get; set; }
        public string ConventionsCasing { get; set; } = string.Empty;
        public int Retries { get; set; }
        public int RetryInterval { get; set; }
        public bool MessagesPersisted { get; set; }
        public ContextOptions? Context { get; set; }
        public ExchangeOptions? Exchange { get; set; }
        public LoggerOptions? Logger { get; set; }
        public SslOptions? Ssl { get; set; }
        public QueueOptions? Queue { get; set; }
        public DeadLetterOptions? DeadLetter { get; set; }
        public QosOptions? Qos { get; set; }
        public ConventionsOptions? Conventions { get; set; }
        public string SpanContextHeader { get; set; } = string.Empty;
        public int MaxProducerChannels { get; set; }
        public bool RequeueFailedMessages { get; set; }

        public string GetSpanContextHeader()
            => string.IsNullOrWhiteSpace(SpanContextHeader) ? "span_context" : SpanContextHeader;

        public class LoggerOptions
        {
            public bool Enabled { get; set; }
            public bool LogConnectionStatus { get; set; }
            public bool LogMessagePayload { get; set; }
        }

        public class ContextOptions
        {
            public bool Enabled { get; set; }
            public string Header { get; set; } = string.Empty;
        }

        public class ExchangeOptions
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public bool Declare { get; set; }
            public bool Durable { get; set; }
            public bool AutoDelete { get; set; }
        }

        public class QueueOptions
        {
            public string Template { get; set; } = string.Empty;
            public bool Declare { get; set; }
            public bool Durable { get; set; }
            public bool Exclusive { get; set; }
            public bool AutoDelete { get; set; }
        }

        public class DeadLetterOptions
        {
            public bool Enabled { get; set; }
            public string Prefix { get; set; } = string.Empty;
            public string Suffix { get; set; } = string.Empty;
            public bool Declare { get; set; }
            public bool Durable { get; set; }
            public bool Exclusive { get; set; }
            public bool AutoDelete { get; set; }
            public int? Ttl { get; set; }
        }

        public class SslOptions
        {
            public bool Enabled { get; set; }
            public string ServerName { get; set; } = string.Empty;
            public string CertificatePath { get; set; } = string.Empty;
            public string CaCertificatePath { get; set; } = string.Empty;
            public IEnumerable<string> X509IgnoredStatuses { get; set; } = new List<string>();
        }

        public class QosOptions
        {
            public uint PrefetchSize { get; set; }
            public ushort PrefetchCount { get; set; }
            public bool Global { get; set; }
        }

        public class ConventionsOptions
        {
            public MessageAttributeOptions? MessageAttribute { get; set; }

            public class MessageAttributeOptions
            {
                public bool IgnoreExchange { get; set; }
                public bool IgnoreRoutingKey { get; set; }
                public bool IgnoreQueue { get; set; }
            }
        }
    }
}
