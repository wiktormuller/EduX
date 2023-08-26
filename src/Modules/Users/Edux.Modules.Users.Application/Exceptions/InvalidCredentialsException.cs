﻿namespace Edux.Modules.Users.Application.Exceptions
{
    internal class InvalidCredentialsException : Shared.Abstractions.Kernel.Exceptions.ApplicationException
    {
        public override string Code { get; } = "invalid_credentials";

        public InvalidCredentialsException() : base("Invalid credentials.")
        {
        }
    }
}
