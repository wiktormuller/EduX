using Bogus;
using Edux.Modules.Users.Core.Exceptions;
using Edux.Modules.Users.Core.ValueObjects;
using Shouldly;

namespace Edux.Modules.Users.Tests.Unit
{
    public class PasswordValueObjectTests
    {
        [Fact]
        public void constructor_when_passing_correct_data_should_create_username()
        {
            // Arrange
            var value = new Faker().Random.String2(10, 200);

            // Act
            var password = new Password(value);

            // Assert
            password.Value.ShouldBe(value);
        }

        [Fact]
        public void constructor_when_passing_value_lesser_than_10_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(9);

            // Act
            var action = () => new Password(value);

            // Assert
            var exception = action.ShouldThrow<InvalidPasswordException>();
            exception.Code.ShouldBe("invalid_password");
        }

        [Fact]
        public void constructor_when_passing_value_greater_than_30_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(201);

            // Act
            var action = () => new Password(value);

            // Assert
            var exception = action.ShouldThrow<InvalidPasswordException>();
            exception.Code.ShouldBe("invalid_password");
        }

        [Fact]
        public void constructor_when_passing_empty_value_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(0);

            // Act
            var action = () => new Password(value);

            // Assert
            var exception = action.ShouldThrow<InvalidPasswordException>();
            exception.Code.ShouldBe("invalid_password");
        }

        [Fact]
        public void constructor_when_passing_null_value_then_should_throws_exception()
        {
            // Arrange
            var value = (string)null;

            // Act
            var action = () => new Password(value);

            // Assert
            var exception = action.ShouldThrow<InvalidPasswordException>();
            exception.Code.ShouldBe("invalid_password");
        }
    }
}
