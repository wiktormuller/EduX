using Edux.Shared.Infrastructure.RabbitMQ.Serializers;

namespace Edux.Shared.Infrastructure.RabbitMQ.Contexts
{
    internal sealed class MessageContextProvider : IMessageContextProvider
    {
        public string HeaderName { get; }

        private readonly IRabbitMqSerializer _serializer;

        public MessageContextProvider(IRabbitMqSerializer serializer, RabbitMqOptions options)
        {
            _serializer = serializer;
            HeaderName = string.IsNullOrWhiteSpace(options?.Context?.Header)
                ? "message_context"
                : options.Context.Header;
        }

        public object Get(IDictionary<string, object> headers)
        {
            if (headers is null)
            {
                return null;
            }

            if (!headers.TryGetValue(HeaderName, out var context))
            {
                return null;
            }

            if (context is byte[] bytes)
            {
                return _serializer.Deserialize(bytes);
            }

            return null;
        }
    }
}
