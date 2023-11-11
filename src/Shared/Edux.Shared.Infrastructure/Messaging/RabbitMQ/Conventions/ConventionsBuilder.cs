using Edux.Shared.Abstractions.Messaging;
using System.Reflection;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions
{
    internal sealed class ConventionsBuilder : IConventionsBuilder
    {
        private readonly RabbitMqOptions _options;
        private readonly bool _useSnakeCase;
        private readonly string _queueTemplate;

        public ConventionsBuilder(RabbitMqOptions options)
        {
            _options = options;

            _useSnakeCase = options?.ConventionsCasing
                ?.Equals("snakeCase", StringComparison.InvariantCultureIgnoreCase) == true;

            _queueTemplate = string.IsNullOrWhiteSpace(_options?.Queue?.Template)
                ? "{{assembly}}/{{exchange}}.{{message}}"
                : options!.Queue!.Template;
        }

        public string GetExchange(Type type)
        {
            var exchange = string.IsNullOrWhiteSpace(_options?.Exchange?.Name)
                ? type.Assembly.GetName().Name!
                : _options.Exchange.Name;

            if (_options?.Conventions?.MessageAttribute?.IgnoreExchange is true)
            {
                return WithCasing(exchange);
            }

            var attribute = GetAttribute(type);
            exchange = string.IsNullOrWhiteSpace(attribute?.Exchange)
                ? exchange
                : attribute.Exchange;

            return WithCasing(exchange);
        }

        public string GetQueue(Type type)
        {
            var attribute = GetAttribute(type);
            var ignoreQueueAttribute = _options?.Conventions?.MessageAttribute?.IgnoreQueue is true;

            if (!ignoreQueueAttribute && !string.IsNullOrWhiteSpace(attribute?.Queue))
            {
                return WithCasing(attribute.Queue);
            }

            var ignoreExchangeAttribute = _options?.Conventions?.MessageAttribute?.IgnoreExchange is true;

            var assembly = type.Assembly.GetName().Name;
            var message = type.Name;

            var exchange = ignoreExchangeAttribute is true
                ? _options?.Exchange?.Name
                : string.IsNullOrWhiteSpace(attribute?.Exchange)
                    ? _options?.Exchange?.Name
                    : attribute.Exchange;

            var queue = _queueTemplate.Replace("{{assembly}}", assembly)
                .Replace("{{exchange}}", exchange)
                .Replace("{{message}}", message);

            return WithCasing(queue);
        }

        public string GetRoutingKey(Type type)
        {
            var routingKey = type.Name;

            if (_options?.Conventions?.MessageAttribute?.IgnoreRoutingKey is true)
            {
                return WithCasing(routingKey);
            }

            var attribute = GetAttribute(type);
            routingKey = string.IsNullOrWhiteSpace(attribute?.RoutingKey)
                ? routingKey
                : attribute.RoutingKey;

            return WithCasing(routingKey);
        }

        private static MessageAttribute? GetAttribute(MemberInfo type)
            => type.GetCustomAttribute<MessageAttribute>();

        private string WithCasing(string value) => _useSnakeCase ? SnakeCase(value) : value;

        private static string SnakeCase(string value)
            => string.Concat(value.Select((x, i) =>
                    i > 0 && value[i - 1] != '.' && value[i - 1] != '/' && char.IsUpper(x) ? "_" + x : x.ToString()))
                .ToLowerInvariant();
    }
}
