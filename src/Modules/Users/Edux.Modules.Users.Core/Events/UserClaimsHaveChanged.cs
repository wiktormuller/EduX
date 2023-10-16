using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.Kernel;

namespace Edux.Modules.Users.Core.Events
{
    public record UserClaimsHaveChanged(User User, DateTime OccurredAt) : IDomainEvent;
}
