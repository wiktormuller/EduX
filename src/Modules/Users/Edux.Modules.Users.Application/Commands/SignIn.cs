using Edux.Shared.Abstractions.Commands;

namespace Edux.Modules.Users.Application.Commands
{
    public record SignIn(string Email, string Password) : ICommand;
}
