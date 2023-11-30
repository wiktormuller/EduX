using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Repositories;
using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Time;

namespace Edux.Modules.Users.Application.Commands.Handlers
{
    public sealed class UpdateUserHandler : ICommandHandler<UpdateUser>
    {
        private readonly IUserRepository _userRepository;
        private readonly IClock _clock;

        public UpdateUserHandler(IUserRepository userRepository,
            IClock clock)
        {
            _userRepository = userRepository;
            _clock = clock;
        }

        public async Task HandleAsync(UpdateUser command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(command.Id)
                ?? throw new UserNotFoundException(command.Id);

            var now = _clock.CurrentDate();

            user.UpdateActivity(command.IsActive, now);

            user.ChangeRole(command.Role, now);

            user.ChangeClaims(command.Claims, now);

            // TOOD: Map Domain events to Integration ones

            _userRepository.Update(user);
        }
    }
}
