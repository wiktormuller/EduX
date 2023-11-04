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

        public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            using var client = new SmtpClient();
            client.Connect(_options.SmtpHost, _options.Port, true, cancellationToken: cancellationToken);
            client.Authenticate(_options.Username, _options.Password, cancellationToken: cancellationToken);

            await client.SendAsync(message, cancellationToken: cancellationToken);
            client.Disconnect(true, cancellationToken: cancellationToken);
        }
    }
}
