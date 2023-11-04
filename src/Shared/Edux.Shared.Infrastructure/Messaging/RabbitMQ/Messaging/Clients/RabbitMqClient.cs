using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Connections;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Serializers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Clients
{
    internal sealed class RabbitMqClient : IRabbitMqClient
    {
        private const string EMPTY_CONTEXT = "{}";

        private readonly IConnection _connection;
        private readonly ILogger<RabbitMqClient> _logger;
        private readonly IRabbitMqSerializer _serializer;
        private readonly IContextAccessor _contextAccessor;
        private readonly IChannelFactory _channelFactory;

        private readonly bool _loggerEnabled;
        private readonly bool _persistMessages;
        private readonly bool _contextEnabled;
        private readonly string _spanContextHeader;
        private readonly string _messageContextHeader;

        public RabbitMqClient(RabbitMqOptions options,
            ProducerConnection connection,
            ILogger<RabbitMqClient> logger,
            IRabbitMqSerializer serializer,
            IContextAccessor contextAccessor,
            IChannelFactory channelFactory)
        {
            _connection = connection.Connection;
            _loggerEnabled = options.Logger?.Enabled ?? false;
            _persistMessages = options?.MessagesPersisted ?? false;
            _contextEnabled = options?.Context?.Enabled ?? false;
            _logger = logger;
            _serializer = serializer;
            _contextAccessor = contextAccessor;
            _spanContextHeader = string.IsNullOrWhiteSpace(options?.SpanContextHeader)
                ? "span_context"
                : options.SpanContextHeader;
            _messageContextHeader = string.IsNullOrWhiteSpace(options?.Context.Header)
                ? "message_context"
                : options.Context.Header;
            _channelFactory = channelFactory;
        }

        public void Send(object message, IConventions conventions, IMessageContext messageContext, string spanContext = null)
        {
            var channel = _channelFactory.Create(_connection);

            var body = _serializer.Serialize(message);

            var properties = BuildProperties(channel, messageContext, spanContext);

            if (_loggerEnabled)
            {
                _logger.LogTrace($"Publishing a message with routing key: '{conventions.RoutingKey}' " +
                             $"to exchange: '{conventions.Exchange}' " +
                             $"[id: '{properties.MessageId}', correlation id: '{properties.CorrelationId}']");
            }

            channel.ConfirmSelect(); // Publisher ACK
            channel.BasicPublish(conventions.Exchange, conventions.RoutingKey, properties, body.ToArray());
        }

        private IBasicProperties BuildProperties(IModel channel, IMessageContext messageContext, 
            string spanContext)
        {
            var properties = channel.CreateBasicProperties();

            properties.Persistent = _persistMessages;

            properties.MessageId = string.IsNullOrWhiteSpace(messageContext.MessageId)
                ? Guid.NewGuid().ToString("N")
                : messageContext.MessageId;

            var correlationId = _contextAccessor?.Context.CorrelationId.ToString("N");
            properties.CorrelationId = correlationId is null
            ? Guid.NewGuid().ToString("N")
            : correlationId;

            properties.Timestamp = new AmqpTimestamp(messageContext.Timestamp);
            properties.Headers = new Dictionary<string, object>();

            if (_contextEnabled)
            {
                if (messageContext is not null)
                {
                    properties.Headers.Add(_messageContextHeader, _serializer.Serialize(messageContext).ToArray());
                }
            }
            else
            {
                properties.Headers.Add(_messageContextHeader, EMPTY_CONTEXT);
            }

            if (!string.IsNullOrWhiteSpace(spanContext))
            {
                properties.Headers.Add(_spanContextHeader, spanContext);
            }

            if (messageContext?.Headers is not null)
            {
                foreach (var (key, value) in messageContext.Headers)
                {
                    if (string.IsNullOrWhiteSpace(key) || value is null)
                    {
                        continue;
                    }

                    properties.Headers.TryAdd(key, value);
                }
            }

            return properties;
        }
    }
}
