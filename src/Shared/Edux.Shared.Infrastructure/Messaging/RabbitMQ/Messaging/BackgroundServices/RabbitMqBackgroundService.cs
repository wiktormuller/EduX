using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Connections;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Channels;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Serializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.BackgroundServices
{
    internal sealed class RabbitMqBackgroundService : BackgroundService
    {
        private readonly MessageSubscriptionsChannel _messageSubscriptionsChannel;
        private readonly ILogger<RabbitMqBackgroundService> _logger;
        private readonly IConventionsProvider _conventionsProvider;
        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRabbitMqSerializer _rabbitMqSerializer;
        private readonly bool _logMessagePayload;
        private readonly IContext _context;

        private readonly ConcurrentDictionary<string, IModel> _channels = new();

        public RabbitMqBackgroundService(MessageSubscriptionsChannel messageSubscriptionsChannel,
            ILogger<RabbitMqBackgroundService> logger,
            IConventionsProvider conventionsProvider,
            ConsumerConnection consumerConnection,
            RabbitMqOptions options,
            IServiceProvider serviceProvider,
            IRabbitMqSerializer rabbitMqSerializer,
            IContext context)
        {
            _messageSubscriptionsChannel = messageSubscriptionsChannel;
            _logger = logger;
            _conventionsProvider = conventionsProvider;
            _connection = consumerConnection.Connection;
            _options = options;
            _serviceProvider = serviceProvider;
            _rabbitMqSerializer = rabbitMqSerializer;
            _logMessagePayload = options?.Logger.LogMessagePayload ?? false;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var messageSubscription in _messageSubscriptionsChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    Subscribe(messageSubscription);
                }
                catch (Exception exception)
                {
                    _logger.LogError($"There was an error during RabbitMQ subscription action.");
                    _logger.LogError(exception, exception.Message);
                }
            }
        }

        private void Subscribe(IMessageSubscription messageSubscription)
        {
            var conventions = _conventionsProvider.Get(messageSubscription.Type);
            var channelKey = $"{conventions.Exchange}:{conventions.Queue}:{conventions.RoutingKey}";

            if (_channels.ContainsKey(channelKey))
            {
                return;
            }

            var channel = _connection.CreateModel();
            var channelInfoLog = $"exchange: '{conventions.Exchange}', queue: '{conventions.Queue}', " +
                                 $"routing key: '{conventions.RoutingKey}'";

            if (!_channels.TryAdd(channelKey, channel))
            {
                _logger.LogError($"Couldn't add the channel for {channelInfoLog}.");
                channel.Dispose();
                return;
            }

            _logger.LogTrace($"Added the channel: {channel.ChannelNumber} for {channelInfoLog}.");

            var declare = _options.Queue?.Declare ?? true;
            var durable = _options.Queue?.Durable ?? true;
            var exclusive = _options.Queue?.Exclusive ?? false;
            var autoDelete = _options.Queue?.AutoDelete ?? false;

            var deadLetterEnabled = _options.DeadLetter?.Enabled is true;
            var deadLetterExchange = deadLetterEnabled
                ? $"{_options.DeadLetter.Prefix}{_options.Exchange.Name}{_options.DeadLetter.Suffix}"
                : string.Empty;
            var deadLetterQueue = deadLetterEnabled
                ? $"{_options.DeadLetter.Prefix}{conventions.Queue}{_options.DeadLetter.Suffix}"
                : string.Empty;

            if (declare)
            {
                _logger.LogInformation($"Declaring a queue: '{conventions.Queue}' with routing key: " +
                                           $"'{conventions.RoutingKey}' for an exchange: '{conventions.Exchange}'.");

                var queueArguments = deadLetterEnabled
                    ? new Dictionary<string, object>
                    {
                    {"x-dead-letter-exchange", deadLetterExchange},
                    {"x-dead-letter-routing-key", deadLetterQueue},
                    }
                    : new Dictionary<string, object>();
                channel.QueueDeclare(conventions.Queue, durable, exclusive, autoDelete, queueArguments);
            }

            channel.QueueBind(conventions.Queue, conventions.Exchange, conventions.RoutingKey);
            channel.BasicQos(_options.Qos.PrefetchSize, _options.Qos.PrefetchCount, _options.Qos.Global);

            if (_options.DeadLetter?.Enabled is true)
            {
                if (_options.DeadLetter.Declare)
                {
                    var ttl = _options.DeadLetter.Ttl.HasValue
                        ? _options.DeadLetter.Ttl <= 0 ? 86400000 : _options.DeadLetter.Ttl
                        : null;
                    var deadLetterArgs = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", conventions.Exchange },
                    { "x-dead-letter-routing-key", conventions.Queue }
                };
                    if (ttl.HasValue)
                    {
                        deadLetterArgs["x-message-ttl"] = ttl.Value;
                    }

                    _logger.LogInformation($"Declaring a dead letter queue: '{deadLetterQueue}' " +
                                           $"for an exchange: '{deadLetterExchange}'{(ttl.HasValue ? $", message TTL: {ttl} ms." : ".")}");

                    channel.QueueDeclare(deadLetterQueue, _options.DeadLetter.Durable, _options.DeadLetter.Exclusive,
                        _options.DeadLetter.AutoDelete, deadLetterArgs);
                }

                channel.QueueBind(deadLetterQueue, deadLetterExchange, deadLetterQueue);
            }

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, args) =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var messageId = args.BasicProperties.MessageId;
                    var correlationId = args.BasicProperties.CorrelationId;
                    var timestamp = args.BasicProperties.Timestamp.UnixTime;
                    var message = _rabbitMqSerializer.Deserialize(args.Body.Span, messageSubscription.Type);

                    var messagePayload = _logMessagePayload
                        ? Encoding.UTF8.GetString(args.Body.Span)
                        : string.Empty;
                    _logger.LogInformation($"Received a message with ID: '{messageId}', Correlation ID: '{correlationId}', " +
                        $"timestamp: {timestamp}, queue: {conventions.Queue}, routing key: {conventions.RoutingKey}, " +
                        $"exchange: {conventions.Exchange}, payload: {messagePayload}");

                    var correlationContext = BuildCorrelationContext(scope, args);
                }
                catch()
                {

                }
            };
        }

        private object BuildCorrelationContext(IServiceScope scope, BasicDeliverEventArgs args)
        {
            var messagePropertiesAccessor = scope.ServiceProvider.GetRequiredService<IContextAccessor>();
            //messagePropertiesAccessor.Context.MessageContext = new 
        }
    }
}
