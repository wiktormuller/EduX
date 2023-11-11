namespace Edux.Modules.Users.Application.Contracts.Responses
{
    public class UserResponse
    {
        public Guid Id { get; }
        public string Email { get; }
        public string Role { get; }
        public bool IsActive { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }
        public Dictionary<string, IEnumerable<string>> Claims { get; } = new();

        public UserResponse(Guid id, string email, string role, bool isActive, DateTime createdAt, DateTime updatedAt,
            Dictionary<string, IEnumerable<string>> claims)
        {
            Id = id;
            Email = email;
            Role = role;
            IsActive = isActive;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Claims = claims;
        }
    }
}
