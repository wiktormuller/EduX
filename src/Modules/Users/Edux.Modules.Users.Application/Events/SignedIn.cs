using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Modules.Users.Application.Events
{
    [Message("users")]
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
