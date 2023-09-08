using Edux.Shared.Abstractions.Serializers;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Edux.Shared.Infrastructure.Serializers
{
    internal class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);
        public T? Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _options);
        public object? Deserialize(string value, Type type) => JsonSerializer.Deserialize(value, type, _options);
    }
}
