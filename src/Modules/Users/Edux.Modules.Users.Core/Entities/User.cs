﻿using Edux.Modules.Users.Core.ValueObjects;
using Edux.Shared.Abstractions.Kernel.Types;

namespace Edux.Modules.Users.Core.Entities
{
    public class User : AggregateRoot
    {
        public AggregateId Id { get; private set; }
        public Email Email { get; private set; }
        public Username Username { get; private set; }
        public Role Role { get; private set; }
        public Password Password { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public Dictionary<string, IEnumerable<string>> Claims { get; private set; }

        protected User()
        {
        }

        public User(Guid id, Email email, Username username, Password password, Role role, bool isActive,
            DateTime createdAt, DateTime updatedAt, 
            Dictionary<string, IEnumerable<string>> claims = null)
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
    }
}
