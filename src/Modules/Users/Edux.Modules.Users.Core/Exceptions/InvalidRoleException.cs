using Edux.Shared.Abstractions.SharedKernel.Exceptions;

namespace Edux.Modules.Users.Core.Exceptions
{
    public class InvalidRoleException : DomainException
    {
        public override string Code { get; } = "invalid_role";

        public InvalidRoleException(string role) : base($"Invalid role: {role}.")
        {
        }
    }
}
