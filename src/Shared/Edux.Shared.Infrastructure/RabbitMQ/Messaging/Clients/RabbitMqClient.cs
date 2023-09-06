using Edux.Shared.Infrastructure.RabbitMQ.Contexts;
using Edux.Shared.Infrastructure.RabbitMQ.Conventions;
using Edux.Shared.Infrastructure.RabbitMQ.Serializers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Edux.Shared.Infrastructure.RabbitMQ.Messaging.Clients
{
    internal sealed class RabbitMqClient : IRabbitMqClient
    {
        private const string EMPTY_CONTEXT = "{}";

        private readonly IConnection _connection;
        private readonly ILogger<RabbitMqClient> _logger;
        private readonly IRabbitMqSerializer _serializer;
        private readonly IContextProvider _contextProvider;

        private readonly bool _loggerEnabled;
        private readonly bool _persistMessages;
        private readonly bool _contextEnabled;
        private readonly string _spanContextHeader;

        private readonly object _lockObject = new object();
        private readonly ConcurrentDictionary<int, IModel> _availableChannels = new();

        public RabbitMqClient(RabbitMqOptions options,
            IConnection connection,
            ILogger<RabbitMqClient> logger,
            IRabbitMqSerializer serializer,
            IContextProvider contextProvider)
        {
            _connection = connection;
            _loggerEnabled = options.Logger?.Enabled ?? false;
            _persistMessages = options?.MessagesPersisted ?? false;
            _contextEnabled = options?.Context?.Enabled ?? false;
            _logger = logger;
            _serializer = serializer;
            _contextProvider = contextProvider;
            _spanContextHeader = string.IsNullOrWhiteSpace(options?.SpanContextHeader)
                ? "span_context"
                : options.SpanContextHeader;
        }

        public void Send(object message, IConventions conventions, string messageId = null, string correlationId = null, 
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
        {
            var currentThreadId = Thread.CurrentThread.ManagedThreadId; // Channels per thread

            if (!_availableChannels.TryGetValue(currentThreadId, out var channel)) // Create new channel
            {
                lock(_lockObject)
                {
                    channel = _connection.CreateModel();
                    _availableChannels.TryAdd(currentThreadId, channel);

                    if (_loggerEnabled)
                    {
                        _logger.LogTrace($"Created a channel for thread: {currentThreadId}.");
                    }
                }
            }
            else // Use already existing channel
            {
                if (_loggerEnabled)
                {
                    _logger.LogTrace($"Reused a channel for thread: {currentThreadId}.");
                }
            }

            var body = _serializer.Serialize(message);

            var properties = BuildProperties(channel, messageId, correlationId, spanContext, messageContext, headers);

            if (_loggerEnabled)
            {
                _logger.LogTrace($"Publishing a message with routing key: '{conventions.RoutingKey}' " +
                             $"to exchange: '{conventions.Exchange}' " +
                             $"[id: '{properties.MessageId}', correlation id: '{properties.CorrelationId}']");
            }

            channel.ConfirmSelect(); // Publisher ACK
            channel.BasicPublish(conventions.Exchange, conventions.RoutingKey, properties, body.ToArray());
        }

        private IBasicProperties BuildProperties(IModel channel, string messageId, string correlationId, string spanContext, 
            object messageContext, IDictionary<string, object> headers)
        {
            var properties = channel.CreateBasicProperties();

            properties.Persistent = _persistMessages;

            properties.MessageId = string.IsNullOrWhiteSpace(messageId)
                ? Guid.NewGuid().ToString("N")
                : messageId;
            properties.CorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? Guid.NewGuid().ToString("N")
            : correlationId;

            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>();

            if (_contextEnabled)
            {
                if (messageContext is not null)
                {
                    properties.Headers.Add(_contextProvider.HeaderName, _serializer.Serialize(messageContext).ToArray());
                }
            }
            else
            {
                properties.Headers.Add(_contextProvider.HeaderName, EMPTY_CONTEXT);
            }

            if (!string.IsNullOrWhiteSpace(spanContext))
            {
                properties.Headers.Add(_spanContextHeader, spanContext);
            }

            if (headers is not null)
            {
                foreach (var (key, value) in headers)
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
