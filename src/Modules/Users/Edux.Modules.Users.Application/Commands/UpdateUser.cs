using Edux.Shared.Abstractions.Commands;

namespace Edux.Modules.Users.Application.Commands
{
    public record UpdateUser(Guid Id, string Role, bool IsActive, Dictionary<string, IEnumerable<string>> Claims) : ICommand;
}
