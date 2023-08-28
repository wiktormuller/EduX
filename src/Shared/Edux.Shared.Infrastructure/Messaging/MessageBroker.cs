using Convey.MessageBrokers;
using Convey.MessageBrokers.RabbitMQ;
using Edux.Shared.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace Edux.Shared.Infrastructure.Messaging
{
    internal sealed class MessageBroker : IMessageBroker
    {
        private const string DefaultSpanContextHeader = "span_context";

        private readonly IBusPublisher _busPublisher;
        private readonly ICorrelationContextAccessor _contextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;
        private readonly ITracer _tracer;
        private readonly ILogger<MessageBroker> _logger;
        private readonly string _spanContextHeader;

        public MessageBroker(IBusPublisher busPublisher, ICorrelationContextAccessor contextAccessor, 
            IHttpContextAccessor httpContextAccessor, IMessagePropertiesAccessor messagePropertiesAccessor, 
            ITracer tracer, ILogger<MessageBroker> logger, RabbitMqOptions options)
        {
            _busPublisher = busPublisher;
            _contextAccessor = contextAccessor;
            _httpContextAccessor = httpContextAccessor;
            _messagePropertiesAccessor = messagePropertiesAccessor;
            _tracer = tracer;
            _logger = logger;
            _spanContextHeader = string.IsNullOrWhiteSpace(options.SpanContextHeader)
                ? DefaultSpanContextHeader
                : options.SpanContextHeader;
        }

        public async Task PublishAsync(params IMessage[] messages)
        {
            if (messages is null)
            {
                return;
            }

            messages = messages.Where(m => m is not null).ToArray();

            if (!messages.Any())
            {
                return;
            }

            // Prepare message to send
            var messageProperties = _messagePropertiesAccessor.MessageProperties;
            var originatedMessageId = messageProperties?.MessageId;
            var correlationId = messageProperties?.CorrelationId;
            
            var spanContext = messageProperties?.GetSpanContext(_spanContextHeader);
            if (string.IsNullOrWhiteSpace(spanContext))
            {
                spanContext = _tracer.ActiveSpan is null
                    ? string.Empty
                    : _tracer.ActiveSpan.Context.ToString();
            }

            var headers = messageProperties.GetHeadersToForward();
            var correlationContext = _contextAccessor.CorrelationContext ??
                                    _httpContextAccessor.GetCorrelationContext();

            foreach (var message in messages)
            {
                var messageId = Guid.NewGuid().ToString("N");
                _logger.LogTrace($"Publishing integration message: {message.GetType().Name} [id: '{messageId}'].");

                await _busPublisher.PublishAsync(message);
            }
        }
    }
}
