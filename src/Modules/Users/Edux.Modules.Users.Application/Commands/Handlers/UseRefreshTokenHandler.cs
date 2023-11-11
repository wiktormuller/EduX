using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Repositories;
using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Commands;

namespace Edux.Modules.Users.Application.Commands.Handlers
{
    internal sealed class UseRefreshTokenHandler : ICommandHandler<UseRefreshToken>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly ITokenStorage _tokenStorage;

        public UseRefreshTokenHandler(IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            ITokenStorage tokenStorage)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _tokenStorage = tokenStorage;
        }

        public async Task HandleAsync(UseRefreshToken command, CancellationToken cancellationToken)
        {
            var token = await _refreshTokenRepository.GetAsync(command.RefreshToken);

            if (token is null)
            {
                throw new InvalidRefreshTokenException();
            }

            if (token.Revoked)
            {
                throw new RevokedRefreshTokenException();
            }

            var user = await _userRepository.GetAsync(token.UserId);

            if (user is null)
            {
                throw new UserNotFoundException(token.UserId);
            }

            var jwtToken = _jwtProvider.CreateToken(token.UserId.ToString(), user.Email.Value, user.Role.Value, claims: user.Claims);
            jwtToken.SetRefreshToken(command.RefreshToken);

            _tokenStorage.Set(jwtToken);
        }
    }
}
