using Edux.Shared.Abstractions.Kernel;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Modules.Users.Application.Services
{
    internal class EventMapper : IEventMapper
    {
        public IMessage Map(IDomainEvent @event)
        {
            return @event switch
            {
                Edux.Modules.Users.Core.Events.UserActivated e
                    => new Edux.Modules.Users.Application.Events.UserActivated(e.User.Id, e.User.Email, e.OccuredAt),

                Edux.Modules.Users.Core.Events.UserDeactivated e
                    => new Edux.Modules.Users.Application.Events.UserDeactivated(e.User.Id, e.User.Email, e.OccuredAt),
            };
        }

        public IEnumerable<IMessage> MapAll(IEnumerable<IDomainEvent> events)
        {
            return events.Select(Map); // It's simplification, because we can map many domain events into one integration event
        }
    }
}
