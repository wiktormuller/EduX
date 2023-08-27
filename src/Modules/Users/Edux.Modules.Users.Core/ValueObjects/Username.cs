using Edux.Modules.Users.Core.Exceptions;

namespace Edux.Modules.Users.Core.ValueObjects
{
    public sealed record Username
    {
        public string Value { get; }

        public Username(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidUsernameException(value);
            }

            if (value.Length > 30 || value.Length < 3)
            {
                throw new InvalidUsernameException(value);
            }

            Value = value;
        }

        public static implicit operator Username(string username) => new(username);

        public static implicit operator string(Username username) => username.Value;

        public override string ToString() => Value;
    }
}
