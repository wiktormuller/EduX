﻿using Edux.Shared.Abstractions.Kernel.Exceptions;

namespace Edux.Modules.Users.Core.Exceptions
{
    public class InvalidUsernameException : DomainException
    {
        public override string Code { get; } = "invalid_username";

        public InvalidUsernameException(string username) : base($"Invalid username: {username}.")
        {
        }
    }
}
