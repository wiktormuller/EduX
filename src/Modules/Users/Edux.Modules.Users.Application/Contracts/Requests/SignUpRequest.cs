namespace Edux.Modules.Users.Application.Contracts.Requests
{
    public class SignUpRequest
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public required Dictionary<string, IEnumerable<string>> Claims { get; set; }
    }
}