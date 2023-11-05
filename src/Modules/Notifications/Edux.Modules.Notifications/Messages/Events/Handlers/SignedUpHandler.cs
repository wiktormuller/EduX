using Edux.Modules.Notifications.Builders;
using Edux.Modules.Notifications.Dto;
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
        private readonly IHubService _hubService;

        public SignedUpHandler(IMessageService messageService, 
            MailKitOptions mailKitOptions, 
            IHubService hubService)
        {
            _messageService = messageService;
            _mailKitOptions = mailKitOptions;
            _hubService = hubService;
        }

        public async Task HandleAsync(SignedUp integrationEvent, CancellationToken cancellationToken)
        {
            var message = MimeMessageBuilder
                .Create()
                .WithReceiver(_ceoEmail)
                .WithSender(_mailKitOptions.Email)
                .WithSubject(MessageTemplates.UserSignedUpSubject, integrationEvent.Email)
                .WithBody(MessageTemplates.UserSignedUpBody, integrationEvent.Email, integrationEvent.CreatedAt)
                .Build();

            await _messageService.SendAsync(message, cancellationToken);

            var userDto = new UserDto(
                integrationEvent.UserId,
                integrationEvent.Email,
                integrationEvent.Role,
                integrationEvent.CreatedAt,
                integrationEvent.Claims);

            await _hubService.PublishUserSignedUpAsync(userDto);
        }
    }
}
