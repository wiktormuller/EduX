using Bogus;
using Edux.Modules.Users.Core.Exceptions;
using Edux.Modules.Users.Core.ValueObjects;
using Shouldly;

namespace Edux.Modules.Users.Tests.Unit.ValueObjects
{
    public class RoleTests
    {
        [Fact]
        public void constructor_when_passing_correct_data_then_should_create_role()
        {
            // Arrange
            var value = "user";

            // Act
            var role = new Role(value);

            // Assert
            role.Value.ShouldBe("user");
        }

        [Fact]
        public void constructor_when_pasing_empty_value_then_should_throws_exception()
        {
            // Arrange
            var value = string.Empty;

            // Act
            var action = () => new Role(value);

            // Assert
            var exception = action.ShouldThrow<InvalidRoleException>();
            exception.Code.ShouldBe("invalid_role");
        }

        [Fact]
        public void constructor_when_pasing_null_value_then_should_throws_exception()
        {
            // Arrange
            var value = (string)null;

            // Act
            var action = () => new Role(value);

            // Assert
            var exception = action.ShouldThrow<InvalidRoleException>();
            exception.Code.ShouldBe("invalid_role");
        }

        [Fact]
        public void constructor_when_passing_not_available_role_then_should_throws_exception()
        {
            // Arrange
            var value = new Faker().Random.String2(10);

            // Act
            var action = () => new Role(value);

            // Assert
            var exception = action.ShouldThrow<InvalidRoleException>();
            exception.Code.ShouldBe("invalid_role");
        }

        [Fact]
        public void admin_when_using_should_create_role_with_admin_value()
        {
            // Act
            var role = Role.Admin();

            // Assert
            role.Value.ShouldBe("admin");
        }

        [Fact]
        public void user_when_using_should_create_role_with_user_value()
        {
            // Act
            var role = Role.User();

            // Assert
            role.Value.ShouldBe("user");
        }
    }
}
