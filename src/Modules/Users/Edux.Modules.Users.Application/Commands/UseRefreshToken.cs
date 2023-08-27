using Edux.Shared.Abstractions.Commands;

namespace Edux.Modules.Users.Application.Commands
{
    public class UseRefreshToken : ICommand
    {
        public string RefreshToken { get; }

        public UseRefreshToken(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
