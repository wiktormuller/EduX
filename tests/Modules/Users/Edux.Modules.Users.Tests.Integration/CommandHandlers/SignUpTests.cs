using Bogus;
using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Commands.Handlers;
using Edux.Modules.Users.Application.Events;
using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Core.ValueObjects;
using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Crypto;
using Edux.Shared.Abstractions.Messaging;
using Edux.Shared.Abstractions.Time;
using NSubstitute;
using Shouldly;

namespace Edux.Modules.Users.Tests.Integration.CommandHandlers
{
    public class SignUpTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IMessageBroker _messageBroker;
        private readonly IClock _clock;

        private readonly SignUpHandler _sut;

        public SignUpTests()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _passwordService = Substitute.For<IPasswordService>();
            _messageBroker = Substitute.For<IMessageBroker>();
            _clock = Substitute.For<IClock>();

            _sut = new SignUpHandler(_userRepository,
                _passwordService,
                _messageBroker,
                _clock);
        }

        [Fact]
        public async Task HandleAsync_When_User_Does_Exist_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var email = faker.Internet.Email();
            var password = faker.Internet.Password();
            var username = faker.Internet.UserName();
            var role = faker.Internet.Random.String2(5, 10);
            var command = new SignUp(email, username, password, role, null);
            var now = DateTime.UtcNow;

            var user = new User(Guid.NewGuid(),
                new Email("user1@email.com"),
                new Username("user1"),
                new Password("Password123!"),
                new Role("user"),
                true,
                now,
                now);

            _userRepository.GetAsync(command.Email).Returns(user);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<EmailAlreadyInUseException>();
            exception.Code.ShouldBe("email_already_in_use");
        }

        [Fact]
        public async Task HandleAsync_When_Passing_Correct_Data_Then_Should_Call_Message_Broker()
        {
            // Arrange
            var faker = new Faker();
            var email = faker.Internet.Email();
            var password = faker.Internet.Password();
            var username = faker.Internet.UserName();
            var role = "user";
            var command = new SignUp(email, username, password, role, null);
            var now = DateTime.UtcNow;
            var passwordHash = faker.Internet.Random.String2(20, 30);

            _userRepository.GetAsync(command.Email).Returns((User)null);
            _passwordService.Hash(command.Password).Returns(passwordHash);
            _clock.CurrentDate().Returns(now);

            // Act
            await _sut.HandleAsync(command, default);

            // Assert
            await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => 
                u.Email == command.Email.ToLowerInvariant() &&
                u.Role.Value == command.Role &&
                u.Password.Value == passwordHash &&
                u.IsActive == true &&
                u.CreatedAt == now &&
                !u.Claims.Any()));

            await _messageBroker.Received(1).PublishAsync(Arg.Is<SignedUp>(message =>
                message.Role == command.Role &&
                message.Email == command.Email.ToLowerInvariant() &&
                message.CreatedAt == now &&
                !message.Claims.Any()));
        }
    }
}
