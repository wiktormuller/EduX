namespace Edux.Modules.Users.Application.Exceptions
{
    internal class EmailAlreadyInUseException : Shared.Abstractions.Kernel.Exceptions.ApplicationException
    {
        public EmailAlreadyInUseException() : base("Email is already in use.")
        {
        }
    }
}
