using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Modules.Users.Application.Services
{
    internal class EventMapper : IEventMapper
    {
        public IMessage Map(IDomainEvent @event)
        {
            return @event switch
            {
                Core.Events.UserActivated e
                    => new Events.UserActivated(e.User.Id, e.User.Email, e.OccurredAt),

                Core.Events.UserDeactivated e
                    => new Events.UserDeactivated(e.User.Id, e.User.Email, e.OccurredAt),
            };
        }

        public IEnumerable<IMessage> MapAll(IEnumerable<IDomainEvent> events)
        {
            return events.Select(Map); // It's simplification, because we can map many domain events into one integration event
        }
    }
}
