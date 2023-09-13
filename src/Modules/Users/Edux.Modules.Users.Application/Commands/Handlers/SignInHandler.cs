using Edux.Modules.Users.Application.Events;
using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Crypto;
using Edux.Shared.Abstractions.Kernel.Types;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Time;

namespace Edux.Modules.Users.Application.Commands.Handlers
{
    internal sealed class SignInHandler : ICommandHandler<SignIn>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly ITokenStorage _tokenStorage;
        private readonly IMessageBroker _messageBroker;
        private readonly IClock _clock;
        private readonly IRandomNumberGenerator _rng;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public SignInHandler(IUserRepository userRepository,
            IJwtProvider jwtProvider,
            IPasswordService passwordService,
            ITokenStorage tokenStorage,
            IClock clock,
            IRandomNumberGenerator rng,
            IRefreshTokenRepository refreshTokenRepository,
            IMessageBroker messageBroker)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _passwordService = passwordService;
            _tokenStorage = tokenStorage;
            _clock = clock;
            _rng = rng;
            _refreshTokenRepository = refreshTokenRepository;
            _messageBroker = messageBroker;
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

            var token = _rng.Generate(30, true);
            var refreshToken = new RefreshToken(Guid.NewGuid(), user.Id, token, _clock.CurrentDate());
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            jwt.RefreshToken = token;

            _tokenStorage.Set(jwt);

            await _messageBroker.PublishAsync(new SignedIn(user.Id, user.Role));
        }
    }
}
