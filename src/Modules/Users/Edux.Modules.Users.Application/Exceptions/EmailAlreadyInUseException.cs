namespace Edux.Modules.Users.Application.Exceptions
{
    internal class EmailAlreadyInUseException : Shared.Abstractions.Kernel.Exceptions.ApplicationException
    {
        public override string Code { get; } = "email_already_in_use";

        public EmailAlreadyInUseException() : base("Email is already in use.")
        {
        }
    }
}
