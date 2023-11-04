using Edux.Shared.Abstraction.Messaging;
using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Infrastructure.Contexts;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Connections;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Channels;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Serializers;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
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
        private readonly bool _requeueFailedMessages;
        private readonly int _retries;
        private readonly int _retryInterval;
        private readonly IBusPublisher _busPublisher;
        private readonly IContextProvider _contextProvider;

        private readonly EmptyExceptionToMessageMapper _exceptionMapper = new();
        private readonly EmptyExceptionToFailedMessageMapper _exceptionFailedMapper = new();

        private readonly IExceptionToMessageMapper _exceptionToMessageMapper;
        private readonly IExceptionToFailedMessageMapper _exceptionToFailedMessageMapper;

        private readonly ConcurrentDictionary<string, IModel> _channels = new();

        public RabbitMqBackgroundService(MessageSubscriptionsChannel messageSubscriptionsChannel,
            ILogger<RabbitMqBackgroundService> logger,
            IConventionsProvider conventionsProvider,
            ConsumerConnection consumerConnection,
            RabbitMqOptions options,
            IServiceProvider serviceProvider,
            IRabbitMqSerializer rabbitMqSerializer,
            IContext context,
            IBusPublisher busPublisher,
            IExceptionToMessageMapper exceptionToMessageMapper,
            IExceptionToFailedMessageMapper exceptionToFailedMessageMapper,
            IContextProvider contextProvider)
        {
            _messageSubscriptionsChannel = messageSubscriptionsChannel;
            _logger = logger;
            _conventionsProvider = conventionsProvider;
            _connection = consumerConnection.Connection;
            _options = options;
            _serviceProvider = serviceProvider;
            _rabbitMqSerializer = rabbitMqSerializer;
            _logMessagePayload = options?.Logger.LogMessagePayload ?? false;
            _requeueFailedMessages = options?.RequeueFailedMessages ?? false;
            _retries = _options.Retries >= 0
                ? _options.Retries
                : 3;
            _retryInterval = _options.RetryInterval > 0
                ? _options.RetryInterval
                : 2;
            _busPublisher = busPublisher;
            _exceptionToMessageMapper = exceptionToMessageMapper;
            _exceptionToFailedMessageMapper = exceptionToFailedMessageMapper;
            _contextProvider = contextProvider;
            _exceptionToMessageMapper = _serviceProvider.GetService<IExceptionToMessageMapper>() 
                ?? _exceptionMapper;
            _exceptionToFailedMessageMapper = _serviceProvider.GetService<IExceptionToFailedMessageMapper>() 
                ?? _exceptionFailedMapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var messageSubscription in _messageSubscriptionsChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    Subscribe(messageSubscription, stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError($"There was an error during RabbitMQ subscription action.");
                    _logger.LogError(exception, exception.Message);
                }
            }
        }

        private void Subscribe(IMessageSubscription messageSubscription, CancellationToken cancellationToken)
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

                    var context = ExtendContext(messageId, args);

                    await TryHandleAsync(channel, message, context, args, messageSubscription.Handler, deadLetterEnabled, cancellationToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    channel.BasicNack(args.DeliveryTag, false, _requeueFailedMessages);
                    await Task.Yield();
                }
            };
        }

        private IContext ExtendContext(string messageId, BasicDeliverEventArgs args)
        {
            var currentContext = _contextProvider.Current();
            var messageContext = new MessageContext(messageId, args.BasicProperties.Headers, 
                args.BasicProperties.Timestamp.UnixTime);
            currentContext.SetMessageContext(messageContext);

            return currentContext;
        }

        private async Task TryHandleAsync(IModel channel, object message, IContext context,
            BasicDeliverEventArgs args, Func<IServiceProvider, object, CancellationToken, Task> handler, 
            bool deadLetterEnabled, CancellationToken cancellationToken)
        {
            var messageName = message.GetType().Name.Underscore();

            var currentRetry = 0;
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, _ => TimeSpan.FromSeconds(_retryInterval));

            var messageContext = context.MessageContext;

            await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger.LogInformation($"Handling a message: {messageName} with ID: {messageContext.MessageId}, " +
                        $"Correlation ID: {context.CorrelationId}, retry: {currentRetry}");

                    if (_options.MessageProcessingTimeout.HasValue)
                    {
                        var task = handler(_serviceProvider, message, cancellationToken);
                        var result = await Task.WhenAny(task, Task.Delay(_options.MessageProcessingTimeout.Value));

                        if (result != task)
                        {
                            throw new RabbitMqMessageProcessingTimeoutException(messageContext.MessageId, context.CorrelationId.ToString("N"));
                        }
                    }
                    else
                    {
                        await handler(_serviceProvider, message, cancellationToken);
                    }

                    channel.BasicAck(args.DeliveryTag, false);
                    await Task.Yield();

                    _logger.LogInformation($"Handled a message: {messageName} with ID: {messageContext.MessageId}, " +
                                           $"Correlation ID: {context.CorrelationId}, retry: {currentRetry}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    if (ex is RabbitMqMessageProcessingTimeoutException)
                    {
                        channel.BasicNack(args.DeliveryTag, false, _requeueFailedMessages);
                        await Task.Yield();
                        return;
                    }

                    currentRetry++;
                    var hasNextRetry = currentRetry <= _retries;

                    var failedMessage = _exceptionToFailedMessageMapper.Map(ex, message);
                    if (failedMessage is null)
                    {
                        // This is a fallback to the previous mechanism in order to avoid the legacy related issues
                        var rejectedEvent = _exceptionToMessageMapper.Map(ex, message);
                        if (rejectedEvent is not null)
                        {
                            failedMessage = new FailedMessage(rejectedEvent, false);
                        }
                    }

                    if (failedMessage?.Message is not null && (!failedMessage.ShouldRetry || !hasNextRetry))
                    {
                        var failedMessageName = failedMessage.Message.GetType().Name.Underscore();
                        var failedMessageId = Guid.NewGuid().ToString("N");
                        await _busPublisher.PublishAsync(failedMessage.Message, failedMessageId, messageContext);
                        _logger.LogError(ex, ex.Message);

                        _logger.LogWarning($"Published a failed messaged: {failedMessageName} with ID: {failedMessageId}, " +
                                               $"Correlation ID: {context.CorrelationId}, for the message: {messageName} with ID: {messageContext.MessageId}");

                        if (!deadLetterEnabled || !failedMessage.MoveToDeadLetter)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            await Task.Yield();
                            return;
                        }
                    }

                    if (failedMessage is null || failedMessage.ShouldRetry)
                    {
                        var errorMessage = $"Unable to handle a message: '{messageName}' with ID: '{messageContext.MessageId}', " +
                                           $"Correlation ID: '{context.CorrelationId}', retry {currentRetry}/{_retries}...";

                        _logger.LogError(errorMessage);

                        if (hasNextRetry)
                        {
                            throw new Exception(errorMessage, ex);
                        }
                    }

                    _logger.LogError($"Handling a message: {messageName} with ID: {messageContext.MessageId}, Correlation ID: " +
                                 $"{context.CorrelationId} failed");

                    if (failedMessage is not null && !failedMessage.MoveToDeadLetter)
                    {
                        channel.BasicAck(args.DeliveryTag, false);
                        await Task.Yield();
                        return;
                    }

                    if (deadLetterEnabled)
                    {
                        _logger.LogError($"Message: {messageName} with ID: {messageContext.MessageId}, Correlation ID: " +
                                         $"{context.CorrelationId} will be moved to DLX");
                    }

                    channel.BasicNack(args.DeliveryTag, false, _requeueFailedMessages);
                    await Task.Yield();
                }
            });
        }
    }
}
