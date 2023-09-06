using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Infrastructure.RabbitMQ.Conventions;
using Edux.Shared.Infrastructure.RabbitMQ.Messaging.Clients;

namespace Edux.Shared.Infrastructure.RabbitMQ.Messaging.Publishers
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

        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null, 
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null) 
                where T : class
        {
            var conventions = _conventionsProvider.Get(message.GetType());
            _rabbitMqClient.Send(message, conventions, messageId, correlationId, spanContext, headers);

            return Task.CompletedTask;
        }
    }
}
