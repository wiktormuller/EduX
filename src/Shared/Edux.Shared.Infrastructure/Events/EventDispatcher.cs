﻿using Edux.Shared.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Events
{
    internal sealed class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            using var scope = _serviceProvider.CreateScope();
            var eventHandlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

            var tasks = eventHandlers.Select(x => x.HandleAsync(@event));

            await Task.WhenAll(tasks); // Synchronous
        }
    }
}
