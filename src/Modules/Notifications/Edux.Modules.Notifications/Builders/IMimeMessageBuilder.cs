using MimeKit;

namespace Edux.Modules.Notifications.Builders
{
    internal interface IMimeMessageBuilder
    {
        IMimeMessageBuilder WithSender(string senderEmail);
        IMimeMessageBuilder WithReceiver(string receiverEmail);
        IMimeMessageBuilder WithSubject(string subject);
        IMimeMessageBuilder WithSubject(string template, params object[] parameters);
        IMimeMessageBuilder WithBody(string body);
        IMimeMessageBuilder WithBody(string template, params object[] parameters);

        MimeMessage Build();
    }
}
