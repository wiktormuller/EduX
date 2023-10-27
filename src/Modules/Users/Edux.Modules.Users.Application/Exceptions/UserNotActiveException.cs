namespace Edux.Modules.Users.Application.Exceptions
{
    internal class UserNotActiveException : Shared.Abstractions.SharedKernel.Exceptions.ApplicationException
    {
        public override string Code { get; } = "user_not_active";

        public UserNotActiveException(string email) : base($"User with Email: '{email}' is not active.")
        {
        }
    }
}
