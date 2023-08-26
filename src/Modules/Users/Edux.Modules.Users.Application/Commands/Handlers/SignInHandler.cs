using Edux.Modules.Users.Application.Events;
using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Repositories;
using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Modules.Users.Application.Commands.Handlers
{
    internal sealed class SignInHandler : ICommandHandler<SignIn>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly IPasswordService _passwordService;
        private readonly ITokenStorage _tokenStorage;
        private readonly IMessageBroker _messageBroker;

        public SignInHandler(IUserRepository userRepository,
            IJwtProvider jwtProvider,
            IRefreshTokenProvider refreshTokenProvider,
            IPasswordService passwordService,
            ITokenStorage tokenStorage)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _refreshTokenProvider = refreshTokenProvider;
            _passwordService = passwordService;
            _tokenStorage = tokenStorage;
        }

        public async Task HandleAsync(SignIn command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(command.Email);

            if (user is null)
            {
                throw new InvalidCredentialsException();
            }

            if (!user.IsActive)
            {
                throw new UserNotActiveException(user.Email);
            }

            if (!_passwordService.IsValid(user.Password, command.Password))
            {
                throw new InvalidCredentialsException();
            }

            var jwt = _jwtProvider.CreateToken(user.Id.ToString(), email: user.Email, role: user.Role, claims: user.Claims);
            jwt.RefreshToken = await _refreshTokenProvider.CreateAsync(user.Id);

            _tokenStorage.Set(jwt);

            await _messageBroker.PublishAsync(new SignedIn(user.Id, user.Role));
        }
    }
}
