using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Events;
using Edux.Modules.Users.Core.ValueObjects;
using Shouldly;

namespace Edux.Modules.Users.Tests.Unit
{
    public class UserAggregateTests
    {
        [Fact]
        public void change_role_when_passing_correct_data_should_change_role_and_emit_event()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var user = new User(Guid.NewGuid(), 
                new Email("user1@email.com"), 
                new Username("user1"), 
                new Password("Password123!"), 
                new Role("user"), 
                true, 
                now, 
                now);

            var occurredAt = now.AddMinutes(1);

            // Act
            user.ChangeRole(new Role("admin"), occurredAt);

            // Assert
            user.Role.Value.ShouldBe("admin");
            user.Events.ShouldNotBeEmpty();
            user.Events.First().ShouldBeOfType<UserRoleHasChanged>();
            var userRoleHasChangedEvent = user.Events.First() as UserRoleHasChanged;
            userRoleHasChangedEvent!.OccuredAt.ShouldBe(occurredAt);
        }
    }
}
