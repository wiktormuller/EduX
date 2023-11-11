using System.Text.Json;
using System.Text.Json.Serialization;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Serializers
{
    internal sealed class SystemTextJsonRabbitMqSerializer : IRabbitMqSerializer
    {
        private readonly JsonSerializerOptions _options;

        public SystemTextJsonRabbitMqSerializer(JsonSerializerOptions? options = null)
        {
            _options = options ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
        }

        public object? Deserialize(ReadOnlySpan<byte> value, Type type)
        {
            return JsonSerializer.Deserialize(value, type, _options);
        }

        public object? Deserialize(ReadOnlySpan<byte> value)
        {
            return JsonSerializer.Deserialize(value, typeof(object), _options);
        }

        public ReadOnlySpan<byte> Serialize(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }
    }
}
