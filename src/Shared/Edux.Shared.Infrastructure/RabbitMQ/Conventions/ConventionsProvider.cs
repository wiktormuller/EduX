using System.Collections.Concurrent;

namespace Edux.Shared.Infrastructure.RabbitMQ.Conventions
{
    internal sealed class ConventionsProvider : IConventionsProvider
    {
        private readonly ConcurrentDictionary<Type, IConventions> _conventions = new();

        private IConventionsRegistry _registry;
        private readonly IConventionsBuilder _builder;

        public IConventions Get<T>()
            => Get(typeof(T));

        public IConventions Get(Type type)
        {
            if (_conventions.TryGetValue(type, out var conventions))
            {
                return conventions;
            }

            var routingKey = _builder.GetRoutingKey(type);
            var exchange = _builder.GetExchange(type);
            var queue = _builder.GetQueue(type);

            conventions = _registry.Get(type)
                ?? new MessageConventions(type, routingKey, exchange, queue);

            _conventions.TryAdd(type, conventions);

            return conventions;
        }
    }
}
