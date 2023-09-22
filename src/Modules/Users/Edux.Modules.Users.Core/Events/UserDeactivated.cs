using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.Kernel;

namespace Edux.Modules.Users.Core.Events
{
    public record UserDeactivated(User User, DateTime OccuredAt) : IDomainEvent;
}
