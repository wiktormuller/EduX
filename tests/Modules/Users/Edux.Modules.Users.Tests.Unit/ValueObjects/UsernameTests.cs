using Bogus;
using Edux.Modules.Users.Core.Exceptions;
using Edux.Modules.Users.Core.ValueObjects;
using Shouldly;

namespace Edux.Modules.Users.Tests.Unit.ValueObjects
{
    public class UsernameTests
    {
        [Fact]
        public void constructor_when_passing_correct_data_should_create_username()
        {
            // Arrange
            var value = new Faker().Random.String2(3, 30);

            // Act
            var username = new Username(value);

            // Assert
            username.Value.ShouldBe(value);
        }

        [Fact]
        public void constructor_when_passing_value_lesser_than_3_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(2);

            // Act
            var action = () => new Username(value);

            // Assert
            var exception = action.ShouldThrow<InvalidUsernameException>();
            exception.Code.ShouldBe("invalid_username");
        }

        [Fact]
        public void constructor_when_passing_value_greater_than_30_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(31);

            // Act
            var action = () => new Username(value);

            // Assert
            var exception = action.ShouldThrow<InvalidUsernameException>();
            exception.Code.ShouldBe("invalid_username");
        }

        [Fact]
        public void constructor_when_passing_empty_value_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(0);

            // Act
            var action = () => new Username(value);

            // Assert
            var exception = action.ShouldThrow<InvalidUsernameException>();
            exception.Code.ShouldBe("invalid_username");
        }

        [Fact]
        public void constructor_when_passing_null_value_then_should_throws_exception()
        {
            // Arrange
            var value = (string)null;

            // Act
            var action = () => new Username(value);

            // Assert
            var exception = action.ShouldThrow<InvalidUsernameException>();
            exception.Code.ShouldBe("invalid_username");
        }
    }
}
