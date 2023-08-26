using Edux.Shared.Abstractions.Events;

namespace Edux.Modules.Users.Application.Events
{
    public class SignedIn : IEvent
    {
        public Guid UserId { get; }
        public string Role { get; }

        public SignedIn(Guid userId, string role)
        {
            UserId = userId;
            Role = role;
        }
    }
}
