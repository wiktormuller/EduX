using Edux.Shared.Abstractions.Kernel.Exceptions;

namespace Edux.Modules.Users.Core.Exceptions
{
    public class InvalidEmailException : DomainException
    {
        public override string Code { get; } = "invalid_email";

        public InvalidEmailException(string email) : base($"Invalid email: {email}.")
        {   
        }
    }
}
