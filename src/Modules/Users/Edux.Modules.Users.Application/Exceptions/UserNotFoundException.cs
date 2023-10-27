namespace Edux.Modules.Users.Application.Exceptions
{
    public class UserNotFoundException : Shared.Abstractions.SharedKernel.Exceptions.ApplicationException
    {
        public override string Code { get; } = "user_not_found";

        public UserNotFoundException(Guid userId) : base($"User with ID: '{userId}' was not found.")
        {
        }
    }
}
