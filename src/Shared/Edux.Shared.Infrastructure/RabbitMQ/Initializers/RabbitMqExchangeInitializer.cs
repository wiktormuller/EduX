using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Infrastructure.RabbitMQ.Connections;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Reflection;

namespace Edux.Shared.Infrastructure.RabbitMQ.Initializers
{
    internal sealed class RabbitMqExchangeInitializer : IInitializer
    {
        private const string DEFAULT_TYPE = "topic";
        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqExchangeInitializer> _logger;
        private readonly bool _loggerEnabled;

        public RabbitMqExchangeInitializer(ProducerConnection connection, RabbitMqOptions options,
        ILogger<RabbitMqExchangeInitializer> logger)
        {
            _connection = connection.Connection;
            _options = options;
            _logger = logger;
            _loggerEnabled = _options?.Logger?.Enabled == true;
        }

        public Task InitializeAsync()
        {
            var exchanges = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsDefined(typeof(MessageAttribute), false))
                .Select(t => t.GetCustomAttribute<MessageAttribute>().Exchange)
                .Distinct()
                .ToList();

            using var channel = _connection.CreateModel();
            if (_options?.Exchange?.Declare is true)
            {
                channel.ExchangeDeclare(_options.Exchange.Name, _options.Exchange.Type, _options.Exchange.Durable,
                    _options.Exchange.AutoDelete);

                if (_options?.DeadLetter?.Enabled is true && _options?.DeadLetter?.Declare is true)
                {
                    channel.ExchangeDeclare($"{_options.DeadLetter.Prefix}{_options.Exchange.Name}{_options.DeadLetter.Suffix}",
                        ExchangeType.Direct, _options.Exchange.Durable, _options.Exchange.AutoDelete);
                }
            }

            foreach (var exchange in exchanges)
            {
                if (exchange.Equals(_options?.Exchange?.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                channel.ExchangeDeclare(exchange, DEFAULT_TYPE, true);
            }

            channel.Close();

            return Task.CompletedTask;
        }
    }
}
