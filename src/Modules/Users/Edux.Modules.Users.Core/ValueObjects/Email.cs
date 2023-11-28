using Edux.Modules.Users.Core.Exceptions;
using System.Net.Mail;

namespace Edux.Modules.Users.Core.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidEmailException(value);
            }

            if (value.Length > 100)
            {
                throw new InvalidEmailException(value);
            }

            value = value.ToLowerInvariant();

            if (!MailAddress.TryCreate(value, out _))
            {
                throw new InvalidEmailException(value);
            }

            Value = value;
        }

        public static implicit operator Email(string email) => new(email);

        public static implicit operator string(Email email) => email.Value;

        public override string ToString() => Value;
    }
}
