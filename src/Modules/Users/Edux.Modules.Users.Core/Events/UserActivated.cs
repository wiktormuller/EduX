using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.Kernel;

namespace Edux.Modules.Users.Core.Events
{
    public record UserActivated(User User, DateTime OccuredAt) : IDomainEvent;
}
