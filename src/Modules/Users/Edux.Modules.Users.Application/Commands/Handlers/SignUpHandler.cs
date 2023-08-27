using Edux.Modules.Users.Application.Events;
using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Core.ValueObjects;
using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Time;

namespace Edux.Modules.Users.Application.Commands.Handlers
{
    internal sealed class SignUpHandler : ICommandHandler<SignUp>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IMessageBroker _messageBroker;
        private readonly IClock _clock;

        public SignUpHandler(IUserRepository userRepository,
            IPasswordService passwordService,
            IMessageBroker messageBroker,
            IClock clock)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _messageBroker = messageBroker;
            _clock = clock;
        }

        public async Task HandleAsync(SignUp command, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetAsync(command.Email);

            if (existingUser is not null)
            {
                throw new EmailAlreadyInUseException();
            }

            var passwordHash = _passwordService.Hash(command.Password);

            var email = new Email(command.Email);
            var password = new Password(passwordHash);
            var role = new Role(command.Role);
            var userName = new Username(command.Username);

            var user = new User(Guid.NewGuid(), email, userName, password, role, isActive: true, 
                _clock.CurrentDate(), _clock.CurrentDate(), command.Claims);
            await _userRepository.AddAsync(user);

            await _messageBroker.PublishAsync(new SignedUp(user.Id, user.Email, user.Role, user.Claims));
        }
    }
}
