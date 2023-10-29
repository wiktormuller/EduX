using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Modules.Notifications.Messages.Events
{
    [Message("users")]
    public class SignedUp : IEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string Role { get; }
        public DateTime CreatedAt { get; }
        public Dictionary<string, IEnumerable<string>> Claims { get; }

        public SignedUp(Guid userId, string email, string role, DateTime createdAt, Dictionary<string, IEnumerable<string>> claims)
        {
            UserId = userId;
            Email = email;
            Role = role;
            CreatedAt = createdAt;
            Claims = claims;
        }
    }
}
