namespace Edux.Modules.Users.Infrastructure.EF.ReadModels
{
    internal class UserReadModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Dictionary<string, IEnumerable<string>> Claims { get; set; }
    }
}
