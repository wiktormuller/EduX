namespace Edux.Modules.Users.Application.Contracts.Requests
{
    public sealed class UpdateUserRequest
    {
        public required string Role { get; set; }
        public required bool IsActive { get; set; }
        public required Dictionary<string, IEnumerable<string>> Claims { get; set; }
    }
}
