using Bogus;
using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Commands.Handlers;
using Edux.Modules.Users.Application.Exceptions;
using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Shared.Abstractions.Time;
using NSubstitute;
using Shouldly;

namespace Edux.Modules.Users.Tests.Integration.CommandHandlers
{
    public class RevokeRefreshTokenTests
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IClock _clock;

        private readonly RevokeRefreshTokenHandler _sut;

        public RevokeRefreshTokenTests()
        {
            _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            _clock = Substitute.For<IClock>();

            _sut = new RevokeRefreshTokenHandler(_refreshTokenRepository, _clock);
        }

        [Fact]
        public async Task HandleAsync_When_Token_Does_Not_Exist_Then_Should_Throws_Exception()
        {
            // Arrange
            var faker = new Faker();
            var token = faker.Internet.Random.String2(10, 20);
            var command = new RevokeRefreshToken(token);

            _refreshTokenRepository.GetAsync(command.RefreshToken).Returns((RefreshToken)null);

            // Act
            var action = () => _sut.HandleAsync(command, default);

            // Assert
            var exception = await action.ShouldThrowAsync<InvalidRefreshTokenException>();
            exception.Code.ShouldBe("invalid_refresh_token");
        }

        [Fact]
        public async Task HandleAsync_When_Passing_Correct_Data_Then_Should_Calls_Repository()
        {
            // Arrange
            var faker = new Faker();
            var token = faker.Internet.Random.String2(10, 20);
            var command = new RevokeRefreshToken(token);
            var now = DateTime.UtcNow;
            var tokenId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var refreshToken = new RefreshToken(tokenId, userId, token, now);

            _refreshTokenRepository.GetAsync(command.RefreshToken).Returns(refreshToken);
            _clock.CurrentDate().Returns(now);

            // Act
            await _sut.HandleAsync(command, default);

            // Assert
            _refreshTokenRepository.Received(1).Update(Arg.Is<RefreshToken>(t =>
                t.Id.Value == tokenId &&
                t.UserId.Value == userId &&
                t.CreatedAt == now &&
                t.RevokedAt == now));
        }
    }
}
