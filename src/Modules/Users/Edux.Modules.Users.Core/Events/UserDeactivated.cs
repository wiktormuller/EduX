using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.SharedKernel;

namespace Edux.Modules.Users.Core.Events
{
    public record UserDeactivated(User User, DateTime OccurredAt) : IDomainEvent;
}
