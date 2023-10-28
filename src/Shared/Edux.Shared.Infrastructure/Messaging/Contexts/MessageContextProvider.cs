using Edux.Shared.Abstractions.Contexts;
using Edux.Shared.Abstractions.Messaging.Contexts;

namespace Edux.Shared.Infrastructure.Messaging.Contexts
{
    internal sealed class MessageContextProvider : IMessageContextProvider
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IMessageContextAccessor _messageContextAccessor;

        public MessageContextProvider(ICorrelationContextAccessor correlationContextAccessor,
            IMessageContextAccessor messageContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _messageContextAccessor = messageContextAccessor;
        }

        public IMessageContext GetCurrent()
        {
            if (_messageContextAccessor.MessageContext is not null)
            {
                return _messageContextAccessor.MessageContext;
            }

            var correlationContext = _correlationContextAccessor.CorrelationContext;
            var context = new MessageContext(correlationContext, null);
            _messageContextAccessor.MessageContext = context;

            return context;
        }

        public void Set(IMessageContext messageContext)
        {
            _messageContextAccessor.MessageContext = messageContext;
        }
    }
}
