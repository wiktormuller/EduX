using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Abstractions.Messaging;
using Edux.Modules.Users.Application.Exceptions;

namespace Edux.Modules.Users.Application.Mappers
{
    internal class EventMapper : IEventMapper
    {
        public IMessage Map(IDomainEvent @event)
        {
            return @event switch
            {
                Core.Events.UserActivated e
                    => new Events.UserActivated(e.User.Id!, e.User.Email, e.OccurredAt),

                Core.Events.UserDeactivated e
                    => new Events.UserDeactivated(e.User.Id!, e.User.Email, e.OccurredAt),

                _ => throw new MappingForDomainEventIsNotDefinedException(@event.GetType().Name)
            };
        }

        public IEnumerable<IMessage> MapAll(IEnumerable<IDomainEvent> events)
        {
            return events.Select(Map); // It's simplification, because we can map many domain events into one integration event
        }
    }
}
