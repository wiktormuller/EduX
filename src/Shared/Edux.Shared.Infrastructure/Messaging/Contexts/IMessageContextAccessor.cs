using Edux.Shared.Abstractions.Messaging.Contexts;

namespace Edux.Shared.Infrastructure.Messaging.Contexts
{
    internal interface IMessageContextAccessor
    {
        IMessageContext? MessageContext { get; set; }
    }
}
