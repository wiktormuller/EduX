using Edux.Shared.Abstractions.Kernel.Exceptions;

namespace Edux.Modules.Users.Core.Exceptions
{
    public class InvalidPasswordException : DomainException
    {
        public override string Code { get; } = "invalid_password";

        public InvalidPasswordException() : base($"Invalid password.")
        {
        }
    }
}
