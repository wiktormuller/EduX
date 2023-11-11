using System.Text;
using System.Text.Json;

namespace Edux.Shared.Infrastructure.Modules.Serializers
{
    internal sealed class JsonModuleSerializer : IModuleSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public T? Deserialize<T>(byte[] value)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(value), SerializerOptions);
        }

        public object? Deserialize(byte[] value, Type type)
        {
            return JsonSerializer.Deserialize(Encoding.UTF8.GetString(value), type, SerializerOptions);
        }

        public byte[] Serialize<T>(T value)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, SerializerOptions));
        }
    }
}
