﻿using Edux.Shared.Abstractions.Commands;

namespace Edux.Modules.Users.Application.Commands
{
    public record SignUp(string Email, string Username, string Password, 
        string Role, Dictionary<string, IEnumerable<string>> Claims) : ICommand;
}
