using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.SharedKernel;

namespace Edux.Modules.Users.Core.Events
{
    public record UserActivated(User User, DateTime OccurredAt) : IDomainEvent;
}
