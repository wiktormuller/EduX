namespace Edux.Shared.Infrastructure.Storage.Mongo.Options
{
    internal sealed class MongoOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
    }
}
