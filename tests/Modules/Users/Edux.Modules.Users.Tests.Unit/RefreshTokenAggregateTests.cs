using Bogus;
using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Exceptions;
using Shouldly;

namespace Edux.Modules.Users.Tests.Unit
{
    public class RefreshTokenAggregateTests
    {
        [Fact]
        public void revoke_when_passing_correct_data_should_should_be_revoked()
        {
            // Arrange
            var token = new Faker().Random.String2(5, 256);
            var now = DateTime.UtcNow;

            var refreshToken = new RefreshToken(Guid.NewGuid(),
                Guid.NewGuid(),
                token,
                now);

            // Act
            refreshToken.Revoke(now);

            // Assert
            refreshToken.Revoked.ShouldBe(true);
            refreshToken.RevokedAt.ShouldBe(now);
        }

        [Fact]
        public void revoke_when_revoking_revoked_token_should_throws_exception()
        {
            // Arrange
            var token = new Faker().Random.String2(5, 256);
            var now = DateTime.UtcNow;

            var refreshToken = new RefreshToken(Guid.NewGuid(),
                Guid.NewGuid(),
                token,
                now,
                now);

            // Act
            var action = () => refreshToken.Revoke(now);

            // Assert
            var exception = action.ShouldThrow<RevokedRefreshTokenException>();
            exception.Code.ShouldBe("revoked_refresh_token");
        }

        [Fact]
        public void constructor_when_passing_token_with_length_lesser_than_5_then_should_throws_exception()
        {
            // Arrange
            var token = new Faker().Random.String2(4);
            var now = DateTime.UtcNow;

            // Act
            var action = () => new RefreshToken(Guid.NewGuid(),
                Guid.NewGuid(),
                token,
                now);

            // Assert
            var exception = action.ShouldThrow<InvalidRefreshTokenException>();
            exception.Code.ShouldBe("invalid_refresh_token");
        }

        [Fact]
        public void constructor_when_passing_token_with_length_greater_than_256_then_should_throws_exception()
        {
            // Arrange
            var token = new Faker().Random.String2(257);
            var now = DateTime.UtcNow;

            // Act
            var action = () => new RefreshToken(Guid.NewGuid(),
                Guid.NewGuid(),
                token,
                now);

            // Assert
            var exception = action.ShouldThrow<InvalidRefreshTokenException>();
            exception.Code.ShouldBe("invalid_refresh_token");
        }

        [Fact]
        public void constructor_when_passing_empty_token_then_should_throws_exception()
        {
            // Arrange
            var token = new Faker().Random.String2(0);
            var now = DateTime.UtcNow;

            // Act
            var action = () => new RefreshToken(Guid.NewGuid(),
                Guid.NewGuid(),
                token,
                now);

            // Assert
            var exception = action.ShouldThrow<EmptyRefreshTokenException>();
            exception.Code.ShouldBe("empty_refresh_token");
        }

        [Fact]
        public void constructor_when_passing_null_token_then_should_throws_exception()
        {
            // Arrange
            var token = (string)null;
            var now = DateTime.UtcNow;

            // Act
            var action = () => new RefreshToken(Guid.NewGuid(),
                Guid.NewGuid(),
                token,
                now);

            // Assert
            var exception = action.ShouldThrow<EmptyRefreshTokenException>();
            exception.Code.ShouldBe("empty_refresh_token");
        }
    }
}
