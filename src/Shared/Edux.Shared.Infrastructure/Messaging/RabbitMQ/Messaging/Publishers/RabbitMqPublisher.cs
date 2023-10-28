using Edux.Shared.Abstractions.Messaging.Contexts;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Clients;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Publishers
{
    internal sealed class RabbitMqPublisher : IBusPublisher
    {
        private readonly IRabbitMqClient _rabbitMqClient;
        private readonly IConventionsProvider _conventionsProvider;

        public RabbitMqPublisher(IRabbitMqClient rabbitMqClient,
            IConventionsProvider conventionsProvider)
        {
            _rabbitMqClient = rabbitMqClient;
            _conventionsProvider = conventionsProvider;
        }

        public Task PublishAsync<T>(T message, string messageId, IMessageContext messageContext,
            string spanContext = null, IDictionary<string, object> headers = null)
                where T : class
        {
            var conventions = _conventionsProvider.Get(message.GetType());
            _rabbitMqClient.Send(message, conventions, messageId, messageContext, spanContext, headers);

            return Task.CompletedTask;
        }
    }
}
