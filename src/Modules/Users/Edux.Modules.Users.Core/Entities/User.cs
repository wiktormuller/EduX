using Edux.Modules.Users.Core.Events;
using Edux.Modules.Users.Core.ValueObjects;
using Edux.Shared.Abstractions.SharedKernel.Types;

namespace Edux.Modules.Users.Core.Entities
{
    public class User : AggregateRoot
    {
        public Email Email { get; private set; }
        public Username Username { get; private set; }
        public Role Role { get; private set; }
        public Password Password { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public Dictionary<string, IEnumerable<string>> Claims { get; private set; }

        private User()
        {
        }

        public User(Guid id, Email email, Username username, Password password, Role role, bool isActive,
            DateTime createdAt, DateTime updatedAt, 
            Dictionary<string, IEnumerable<string>>? claims = null)
        {
            Id = id;
            Email = email;
            Username = username;
            Password = password;
            Role = role;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            IsActive = isActive;
            Claims = claims ?? new Dictionary<string, IEnumerable<string>>();
        }

        public void ChangeRole(Role role, DateTime occurredAt)
        {
            if (Role.Value == role.Value) // Idempotent
            {
                return;
            }

            Role = role;
            UpdatedAt = occurredAt;
            AddEvent(new UserRoleHasChanged(this, occurredAt));
        }

        public void ChangeClaims(Dictionary<string, IEnumerable<string>> claims, DateTime occurredAt)
        {
            if (AreClaimsEqual(Claims, claims)) // Idempotent
            {
                return;
            }

            Claims = claims;
            UpdatedAt = occurredAt;
            AddEvent(new UserClaimsHaveChanged(this, occurredAt));
        }

        public void UpdateActivity(bool isActive, DateTime occurredAt)
        {
            if (IsActive == isActive) // Idempotent
            {
                return;
            }

            if (isActive)
            {
                Activate(occurredAt);
            }
            else
            {
                Deactivate(occurredAt);
            }
        }

        private void Activate(DateTime occurredAt)
        {
            if (!IsActive)
            {
                IsActive = true;
                UpdatedAt = occurredAt;
                AddEvent(new UserActivated(this, occurredAt));
            }
        }

        private void Deactivate(DateTime occurredAt)
        {
            if (IsActive)
            {
                IsActive = false;
                UpdatedAt = occurredAt;
                AddEvent(new UserDeactivated(this, occurredAt));
            }
        }

        private bool AreClaimsEqual(Dictionary<string, IEnumerable<string>> first,
            Dictionary<string, IEnumerable<string>> second)
        {
            // Check if the number of elements is the same
            if (first.Count != second.Count)
            {
                return false;
            }

            // Check if all keys in first are present in second
            if (!first.Keys.All(key => second.ContainsKey(key)))
            {
                return false;
            }

            // Check if the values for corresponding keys are equal
            foreach (var kvp in first)
            {
                if (!second.TryGetValue(kvp.Key, out IEnumerable<string>? value) || !kvp.Value.SequenceEqual(value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
