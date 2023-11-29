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
    public class SignInTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordService _passwordService;
        private readonly ITokenStorage _tokenStorage;
        private readonly IMessageBroker _messageBroker;
        private readonly IClock _clock;
        private readonly IRandomNumberGenerator _rng;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        private readonly SignInHandler _sut;

        public SignInTests()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _jwtProvider = Substitute.For<IJwtProvider>();
            _passwordService = Substitute.For<IPasswordService>();
            _tokenStorage = Substitute.For<ITokenStorage>();
            _messageBroker = Substitute.For<IMessageBroker>();
            _clock = Substitute.For<IClock>();
            _rng = Substitute.For<IRandomNumberGenerator>();
            _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

            _sut = new SignInHandler(_userRepository,
                _jwtProvider,
                _passwordService,
                _tokenStorage,
                _clock,
                _rng,
                _refreshTokenRepository,
                _messageBroker);
        }

        [Fact]
        public async Task HandleAsync_When_User_Does_Not_Exist_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var email = faker.Internet.Email();
            var password = faker.Internet.Password();
            var command = new SignIn(email, password);

            _userRepository.GetAsync(command.Email).Returns((User)null);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<InvalidCredentialsException>();
            exception.Code.ShouldBe("invalid_credentials");
        }

        [Fact]
        public async Task HandleAsync_When_User_Is_Not_Active_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var email = faker.Internet.Email();
            var password = faker.Internet.Password();
            var command = new SignIn(email, password);

            var isActive = false;
            var now = DateTime.UtcNow;
            var user = new User(Guid.NewGuid(),
                new Email("user1@email.com"),
                new Username("user1"),
                new Password("Password123!"),
                new Role("user"),
                isActive,
                now,
                now);

            _userRepository.GetAsync(command.Email).Returns(user);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<UserNotActiveException>();
            exception.Code.ShouldBe("user_not_active");
        }

        [Fact]
        public async Task HandleAsync_When_Password_Is_Incorrect_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var email = faker.Internet.Email();
            var password = faker.Internet.Password();
            var command = new SignIn(email, password);

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
            _passwordService.IsValid(user.Password, command.Password).Returns(false);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<InvalidCredentialsException>();
            exception.Code.ShouldBe("invalid_credentials");
        }

        [Fact]
        public async Task HandleAsync_When_Passing_Correct_Data_Then_Should_Call_Message_Broker()
        {
            // Arrange
            var faker = new Faker();
            var email = faker.Internet.Email();
            var password = faker.Internet.Password();
            var command = new SignIn(email, password);

            var now = DateTime.UtcNow;
            var user = new User(Guid.NewGuid(),
                new Email("user1@email.com"),
                new Username("user1"),
                new Password("Password123!"),
                new Role("user"),
                true,
                now,
                now);

            var token = new JsonWebToken("accessToken", "refreshToken", 12332L, "someId", "someRole", "someEmail", null);

            _userRepository.GetAsync(command.Email).Returns(user);
            _passwordService.IsValid(user.Password, command.Password).Returns(true);
            _jwtProvider.CreateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), claims: Arg.Any<IDictionary<string, IEnumerable<string>>>()).Returns(token);
            _rng.Generate(Arg.Any<int>()).Returns("someRng");
            _clock.CurrentDate().Returns(now);

            // Act
            await _sut.HandleAsync(command, default);

            // Assert
            await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>());
            _tokenStorage.Received(1).Set(Arg.Any<JsonWebToken>());
            await _messageBroker.Received(1).PublishAsync(Arg.Is<SignedIn>(message => message.UserId == user.Id.Value && message.Role == user.Role));
        }
    }
}
