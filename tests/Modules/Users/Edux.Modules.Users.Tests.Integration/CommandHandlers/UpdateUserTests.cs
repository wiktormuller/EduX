using Bogus;
using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Commands.Handlers;
using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Core.ValueObjects;
using Edux.Shared.Abstractions.Time;
using NSubstitute;
using Shouldly;

namespace Edux.Modules.Users.Tests.Integration.CommandHandlers
{
    public class UpdateUserTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IClock _clock;

        private readonly UpdateUserHandler _sut;

        public UpdateUserTests()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _clock = Substitute.For<IClock>();

            _sut = new UpdateUserHandler(_userRepository, _clock);
        }

        [Fact]
        public async Task HandleAsync_When_User_Does_Not_Exist_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var role = "user";
            var id = Guid.NewGuid();
            var command = new UpdateUser(id, role, true, new Dictionary<string, IEnumerable<string>>());

            _userRepository.GetAsync(command.Id).Returns((User)null);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<UserNotFoundException>();
            exception.Code.ShouldBe("user_not_found");
        }

        [Fact]
        public async Task HandleAsync_When_Passing_Correct_Data_Then_Should_Calls_Repository()
        {
            // Arrange
            var faker = new Faker();
            var role = "user";
            var id = Guid.NewGuid();
            var command = new UpdateUser(id, role, false, new Dictionary<string, IEnumerable<string>>());
            var now = DateTime.UtcNow;

            var user = new User(id,
                new Email("user1@email.com"),
                new Username("user1"),
                new Password("Password123!"),
                new Role("user"),
                true,
                now,
                now);

            _userRepository.GetAsync(command.Id).Returns(user);
            _clock.CurrentDate().Returns(now);

            // Act
            await _sut.HandleAsync(command, default);

            // Assert
            _userRepository.Received(1).Update(Arg.Is<User>(u =>
                u.Role.Value == command.Role &&
                u.Id.Value == command.Id &&
                u.IsActive == false &&
                u.UpdatedAt == now &&
                !u.Claims.Any()));
        }
    }
}
