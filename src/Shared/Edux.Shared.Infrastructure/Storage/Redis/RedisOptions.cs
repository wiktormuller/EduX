namespace Edux.Shared.Infrastructure.Storage.Redis
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public int Database { get; set; }
    }
}
