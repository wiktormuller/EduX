using Edux.Shared.Abstraction.Observability.Metrics;
using Edux.Shared.Abstractions.Messaging;
using Humanizer;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Edux.Shared.Infrastructure.Observability.Metrics.Decorators
{
    [MeterAttribute("message_broker")]
    internal sealed class MessageBrokerMetricsDecorator : IMessageBroker
    {
        private readonly IMessageBroker _messageBroker;
        private static readonly ConcurrentDictionary<Type, string> Names = new();
        private static readonly Meter Meter = new("message_broker");
        private static readonly Counter<long> PublishedMessagesCounter = Meter.CreateCounter<long>("published_messages");

        public MessageBrokerMetricsDecorator(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public async Task PublishAsync(params IMessage[] messages)
        {
            await _messageBroker.PublishAsync(messages);

            foreach (var message in messages)
            {
                var name = Names.GetOrAdd(message.GetType(), message.GetType().Name.Underscore());
                var tags = new KeyValuePair<string, object?>[]
                {
                    new ("message", name)
                };
                
                PublishedMessagesCounter.Add(messages.Length, tags);
            }
        }
    }
}
