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
            userRoleHasChangedEvent!.OccurredAt.ShouldBe(occurredAt);
            user.UpdatedAt.ShouldBe(occurredAt);
        }

        [Fact]
        public void change_claims_when_passing_correct_data_should_change_claims_and_emit_event()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var claims = new Dictionary<string, IEnumerable<string>>()
            {
                { "permissions", new List<string>() { "users" } }
            };

            var newClaims = new Dictionary<string, IEnumerable<string>>()
            {
                { "permissions", new List<string>() { "notifications" } }
            };

            var user = new User(Guid.NewGuid(),
                new Email("user1@email.com"),
                new Username("user1"),
                new Password("Password123!"),
                new Role("user"),
                true,
                now,
                now,
                claims);

            var occurredAt = now.AddMinutes(1);

            // Act
            user.ChangeClaims(newClaims, occurredAt);

            // Assert
            user.Claims.FirstOrDefault()
                .Value
                .FirstOrDefault()
                .ShouldBe("notifications");

            user.Events.ShouldNotBeEmpty();
            user.Events.First().ShouldBeOfType<UserClaimsHaveChanged>();
            var userClaimsHaveChangedEvent = user.Events.First() as UserClaimsHaveChanged;
            userClaimsHaveChangedEvent!.OccurredAt.ShouldBe(occurredAt);
            user.UpdatedAt.ShouldBe(occurredAt);
        }

        [Fact]
        public void update_activity_when_passing_correct_data_should_change_activity_and_emit_event()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var isActive = true;
            var newActivityStatus = false;

            var user = new User(Guid.NewGuid(),
                new Email("user1@email.com"),
                new Username("user1"),
                new Password("Password123!"),
                new Role("user"),
                isActive,
                now,
                now);

            var occurredAt = now.AddMinutes(1);

            // Act
            user.UpdateActivity(newActivityStatus, occurredAt);

            // Assert
            user.IsActive.ShouldBe(false);

            user.Events.ShouldNotBeEmpty();
            user.Events.First().ShouldBeOfType<UserDeactivated>();
            var userActivatedEvent = user.Events.First() as UserDeactivated;
            userActivatedEvent!.OccurredAt.ShouldBe(occurredAt);
            user.UpdatedAt.ShouldBe(occurredAt);
        }
    }
}
