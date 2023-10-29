namespace Edux.Shared.Infrastructure.Messaging.MailKit
{
    public sealed class MailKitOptions
    {
        public string SmtpHost { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
