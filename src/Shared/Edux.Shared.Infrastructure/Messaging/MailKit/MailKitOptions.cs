namespace Edux.Shared.Infrastructure.Messaging.MailKit
{
    public sealed class MailKitOptions
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
