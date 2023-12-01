using Bogus;
using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Commands.Handlers;
using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Core.ValueObjects;
using Edux.Shared.Abstractions.Auth;
using NSubstitute;
using Shouldly;

namespace Edux.Modules.Users.Tests.Integration.CommandHandlers
{
    public class UseRefreshTokenTests
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly ITokenStorage _tokenStorage;

        private readonly UseRefreshTokenHandler _sut;

        public UseRefreshTokenTests()
        {
            _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            _userRepository = Substitute.For<IUserRepository>();
            _jwtProvider = Substitute.For<IJwtProvider>();
            _tokenStorage = Substitute.For<ITokenStorage>();

            _sut = new UseRefreshTokenHandler(_refreshTokenRepository,
                _userRepository,
                _jwtProvider,
                _tokenStorage);
        }

        [Fact]
        public async Task HandleAsync_When_Token_Does_Not_Exist_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var token = faker.Internet.Random.String2(10, 20);
            var command = new UseRefreshToken(token);

            _refreshTokenRepository.GetAsync(command.RefreshToken).Returns((RefreshToken)null);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<InvalidRefreshTokenException>();
            exception.Code.ShouldBe("invalid_refresh_token");
        }

        [Fact]
        public async Task HandleAsync_When_Token_Is_Revoked_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var token = faker.Internet.Random.String2(10, 20);
            var command = new UseRefreshToken(token);
            var now = DateTime.UtcNow;

            var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), token, now, now);

            _refreshTokenRepository.GetAsync(command.RefreshToken).Returns(refreshToken);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<RevokedRefreshTokenException>();
            exception.Code.ShouldBe("revoked_refresh_token");
        }

        [Fact]
        public async Task HandleAsync_When_User_Does_Not_Exist_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var token = faker.Internet.Random.String2(10, 20);
            var command = new UseRefreshToken(token);
            var now = DateTime.UtcNow;

            var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), token, now);

            _refreshTokenRepository.GetAsync(command.RefreshToken).Returns(refreshToken);
            _userRepository.GetAsync(refreshToken.UserId).Returns((User)null);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<UserNotFoundException>();
            exception.Code.ShouldBe("user_not_found");
        }

        [Fact]
        public async Task HandleAsync_When_Passing_Correct_Data_Then_Should_Calls_Token_Storage()
        {
            // Arrange
            var faker = new Faker();
            var token = faker.Internet.Random.String2(10, 20);
            var command = new UseRefreshToken(token);
            var now = DateTime.UtcNow;

            var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), token, now);
            var user = new User(Guid.NewGuid(),
                new Email("user1@email.com"),
                new Username("user1"),
                new Password("Password123!"),
                new Role("user"),
                true,
                now,
                now);

            var jwtToken = new JsonWebToken("accessToken", token, 12332L, "someId", "someRole", "someEmail", null);

            _refreshTokenRepository.GetAsync(command.RefreshToken).Returns(refreshToken);
            _userRepository.GetAsync(refreshToken.UserId).Returns(user);
            _jwtProvider.CreateToken(refreshToken.UserId.ToString(), user.Email, user.Role, claims: user.Claims)
                .Returns(jwtToken);

            // Act
            await _sut.HandleAsync(command, default);

            // Assert
            _tokenStorage.Received(1).Set(Arg.Is<JsonWebToken>(jwt =>
                jwt.AccessToken == "accessToken" &&
                jwt.RefreshToken == token &&
                jwt.Expires == 12332 &&
                jwt.Id == "someId" &&
                jwt.Role == "someRole" &&
                jwt.Email == "someEmail" &&
                jwt.Claims == null));
        }
    }
}
