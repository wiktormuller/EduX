using Edux.Modules.Notifications.Builders;
using Edux.Modules.Notifications.Services;
using Edux.Modules.Notifications.Templates;
using Edux.Shared.Abstractions.Events;
using Edux.Shared.Infrastructure.Messaging.MailKit;

namespace Edux.Modules.Notifications.Messages.Events.Handlers
{
    internal sealed class SignedUpHandler : IEventHandler<SignedUp>
    {
        private readonly IMessageService _messageService;
        private readonly MailKitOptions _mailKitOptions;
        private const string _ceoEmail = "ceo@edux.com";

        public SignedUpHandler(IMessageService messageService, MailKitOptions mailKitOptions)
        {
            _messageService = messageService;
            _mailKitOptions = mailKitOptions;
        }

        public async Task HandleAsync(SignedUp @event)
        {
            var message = MimeMessageBuilder
                .Create()
                .WithReceiver(_ceoEmail)
                .WithSender(_mailKitOptions.Email)
                .WithSubject(MessageTemplates.UserSignedUpSubject, @event.Email)
                .WithBody(MessageTemplates.UserSignedUpBody, @event.Email, @event.CreatedAt)
                .Build();

            await _messageService.SendAsync(message);
        }
    }
}
