namespace Edux.Shared.Infrastructure.HTTP.Options
{
    public sealed class HttpClientOptions
    {
        public string Name { get; set; } = string.Empty;
        public CertificateOptions? Certificate { get; set; }
        public ResiliencyOptions Resiliency { get; set; } = new();

        public sealed class CertificateOptions
        {
            public string Location { get; set; } = string.Empty;
            public string? Password { get; set; }
        }

        public sealed class ResiliencyOptions
        {
            public int Retries { get; set; } = 3;
            public TimeSpan? RetryInterval { get; set; }
            public bool Exponential { get; set; }
        }
    }
}
