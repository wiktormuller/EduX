using Edux.Shared.Abstractions.Events;

namespace Edux.Modules.Users.Application.Events
{
    public class SignedUp : IEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string Role { get; }
        public Dictionary<string, IEnumerable<string>> Claims { get; }

        public SignedUp(Guid userId, string email, string role, Dictionary<string, IEnumerable<string>> claims)
        {
            UserId = userId;
            Email = email;
            Role = role;
            Claims = claims;
        }
    }
}
