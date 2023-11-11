using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Repositories;
using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Time;

namespace Edux.Modules.Users.Application.Commands.Handlers
{
    internal sealed class RevokeRefreshTokenHandler : ICommandHandler<RevokeRefreshToken>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IClock _clock;

        public RevokeRefreshTokenHandler(IRefreshTokenRepository refreshTokenRepository,
            IClock clock)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _clock = clock;
        }

        public async Task HandleAsync(RevokeRefreshToken command, CancellationToken cancellationToken)
        {
            var refreshToken = await _refreshTokenRepository.GetAsync(command.RefreshToken);

            if (refreshToken is null)
            {
                throw new InvalidRefreshTokenException();
            }

            refreshToken.Revoke(_clock.CurrentDate());

            _refreshTokenRepository.Update(refreshToken);
        }
    }
}
