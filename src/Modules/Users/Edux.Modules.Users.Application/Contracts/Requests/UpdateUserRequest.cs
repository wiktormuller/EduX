namespace Edux.Modules.Users.Application.Contracts.Requests
{
    public sealed class UpdateUserRequest
    {
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, IEnumerable<string>> Claims { get; set; }
    }
}
