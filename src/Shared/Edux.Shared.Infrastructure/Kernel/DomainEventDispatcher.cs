using Edux.Shared.Abstractions.Kernel;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Kernel
{
    internal sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync(params IDomainEvent[] events)
        {
            if (events is null || !events.Any())
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            foreach (var domainEvent in events)
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handlers = scope.ServiceProvider.GetServices(handlerType);

                var tasks = handlers.Select(x => (Task)handlerType
                    .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
                    .Invoke(x, new[] { domainEvent }));

                await Task.WhenAll(tasks);
            }
        }
    }
}
