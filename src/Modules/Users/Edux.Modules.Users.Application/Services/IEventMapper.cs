﻿using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Abstractions.Messaging;

namespace Edux.Modules.Users.Application.Services
{
    public interface IEventMapper
    { 
        IMessage Map(IDomainEvent @event);
        IEnumerable<IMessage> MapAll(IEnumerable<IDomainEvent> events);
    }
}
