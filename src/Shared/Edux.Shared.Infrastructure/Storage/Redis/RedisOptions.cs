﻿namespace Edux.Shared.Infrastructure.Storage.Redis
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = "localhost";
        public string Instance { get; set; }
        public int Database { get; set; }
    }
}
