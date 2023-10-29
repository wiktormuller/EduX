using MimeKit;

namespace Edux.Modules.Notifications.Services
{
    internal interface IMessageService
    {
        Task SendAsync(MimeMessage message);
    }
}
