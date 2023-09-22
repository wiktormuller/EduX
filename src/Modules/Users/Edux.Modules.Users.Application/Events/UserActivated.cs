﻿using Edux.Shared.Abstractions.Events;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Modules.Users.Application.Events
{
    [Message("users")]
    internal class UserActivated : IEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime OccuredAt { get; }

        public UserActivated(Guid userId, string email, DateTime occuredAt)
        {
            UserId = userId;
            Email = email;
            OccuredAt = occuredAt;
        }
    }
}
