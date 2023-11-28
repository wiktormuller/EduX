using Bogus;
using Edux.Modules.Users.Core.Exceptions;
using Edux.Modules.Users.Core.ValueObjects;
using Shouldly;

namespace Edux.Modules.Users.Tests.Unit.ValueObjects
{
    public class EmailTests
    {
        [Fact]
        public void constructor_when_passing_correct_data_then_should_create_username()
        {
            // Arrange
            var faker = new Faker();
            var value = faker
                .Internet
                .Email(null, faker.Random.String2(3, 100))
                .ToLowerInvariant();

            // Act
            var email = new Email(value);

            // Assert
            email.Value.ShouldBe(value);
        }

        [Fact]
        public void constructor_when_passing_value_lesser_than_3_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(2);

            // Act
            var action = () => new Email(value);

            // Assert
            var exception = action.ShouldThrow<InvalidEmailException>();
            exception.Code.ShouldBe("invalid_email");
        }

        [Fact]
        public void constructor_when_passing_value_greater_than_100_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(201);

            // Act
            var action = () => new Email(value);

            // Assert
            var exception = action.ShouldThrow<InvalidEmailException>();
            exception.Code.ShouldBe("invalid_email");
        }

        [Fact]
        public void constructor_when_passing_empty_value_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(0);

            // Act
            var action = () => new Email(value);

            // Assert
            var exception = action.ShouldThrow<InvalidEmailException>();
            exception.Code.ShouldBe("invalid_email");
        }

        [Fact]
        public void constructor_when_passing_null_value_then_should_throws_exception()
        {
            // Arrange
            var value = (string)null;

            // Act
            var action = () => new Email(value);

            // Assert
            var exception = action.ShouldThrow<InvalidEmailException>();
            exception.Code.ShouldBe("invalid_email");
        }

        [Fact]
        public void constructor_when_passing_value_without_at_sign_then_should_throw_exception()
        {
            // Arrange
            var value = "someInvalidEmail.com";

            // Act
            var action = () => new Email(value);

            // Assert
            var exception = action.ShouldThrow<InvalidEmailException>();
            exception.Code.ShouldBe("invalid_email");
        }
    }
}
