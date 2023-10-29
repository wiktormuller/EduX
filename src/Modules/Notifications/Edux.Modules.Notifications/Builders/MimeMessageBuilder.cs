using MimeKit;
using System.Text;

namespace Edux.Modules.Notifications.Builders
{
    internal sealed class MimeMessageBuilder : IMimeMessageBuilder
    {
        private readonly MimeMessage _message;
        private MimeMessageBuilder()
        {
            _message = new MimeMessage();
        }
        // An interface type definition can define and implement static methods(see §8.4.3)
        // since static methods are associated with the interface type itself rather than with any value of the type.
        // Bear in mind that static members are usually utility methods.
        // We can use abstract static methods in interface, but then we need to call them explicitly via interface type.
        public static IMimeMessageBuilder Create()
        {
            return new MimeMessageBuilder();
        }

        public IMimeMessageBuilder WithBody(string body)
        {
            _message.Body = new TextPart("plain")
            {
                Text = body
            };
            return this;
        }

        public IMimeMessageBuilder WithBody(string template, params object[] parameters)
        {
            return WithBody(string.Format(template, parameters));   
        }

        public IMimeMessageBuilder WithReceiver(string receiverEmail)
        {
            _message.To.Add(new MailboxAddress(Encoding.UTF8, null, receiverEmail));
            return this;
        }

        public IMimeMessageBuilder WithSender(string senderEmail)
        {
            _message.From.Add(new MailboxAddress(Encoding.UTF8, null,senderEmail));
            return this;
        }

        public IMimeMessageBuilder WithSubject(string subject)
        {
            _message.Subject = subject;
            return this;
        }

        public IMimeMessageBuilder WithSubject(string template, params object[] parameters)
        {
            return WithSubject(string.Format(template, parameters));
        }

        public MimeMessage Build()
        {
            return _message;
        }
    }
}
