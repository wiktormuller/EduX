namespace Edux.Modules.Notifications.Dto
{
    public sealed class UserDto
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string Role { get; }
        public DateTime CreatedAt { get; }
        public Dictionary<string, IEnumerable<string>> Claims { get; }

        public UserDto(Guid userId, string email, string role, 
            DateTime createdAt, Dictionary<string, IEnumerable<string>> claims)
        {
            UserId = userId;
            Email = email;
            Role = role;
            CreatedAt = createdAt;
            Claims = claims;
        }
    }
}
