using MimeKit;
using MailKit.Net.Smtp;
using Edux.Shared.Infrastructure.Messaging.MailKit;

namespace Edux.Modules.Notifications.Services
{
    internal sealed class MessageService : IMessageService
    {
        private readonly MailKitOptions _options;

        public MessageService(MailKitOptions options)
        {
            _options = options;
        }

        public async Task SendAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            client.Connect(_options.SmtpHost, _options.Port, true);
            client.Authenticate(_options.Username, _options.Password);

            await client.SendAsync(message);
            client.Disconnect(true);
        }
    }
}
