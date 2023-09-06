namespace Edux.Shared.Infrastructure.RabbitMQ.Serializers
{
    internal interface IRabbitMqSerializer
    {
        ReadOnlySpan<byte> Serialize(object value);
        object Deserialize(ReadOnlySpan<byte> value, Type type);
        object Deserialize(ReadOnlySpan<byte> value);
    }
}
