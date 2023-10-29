using Edux.Shared.Abstractions.Commands;

namespace Edux.Modules.Notifications.Messages.Commands
{
    internal sealed class SendEmailNotification : ICommand
    {
        public string Email { get; }
        public string Title { get; }
        public string Message { get; }

        public SendEmailNotification(string email, string title, string message)
        {
            Email = email;
            Title = title;
            Message = message;
        }
    }
}
