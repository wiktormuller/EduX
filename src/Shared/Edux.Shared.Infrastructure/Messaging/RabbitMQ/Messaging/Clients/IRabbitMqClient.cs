﻿using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Clients
{
    internal interface IRabbitMqClient
    {
        void Send(object message, IConventions conventions, IMessageContext messageContext,
        string? spanContext = null);
    }
}
