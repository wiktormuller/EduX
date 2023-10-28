using Edux.Shared.Abstractions.Messaging.Contexts;

namespace Edux.Shared.Infrastructure.Messaging.Contexts
{
    internal sealed class MessageContextAccessor : IMessageContextAccessor
    {
        private static readonly AsyncLocal<MessageContextHolder> Holder = new();

        public IMessageContext? MessageContext
        {
            get => Holder.Value?.Context;
            set
            {
                var holder = Holder.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value is not null)
                {
                    Holder.Value = new MessageContextHolder { Context = value };
                }
            }
        }

        private class MessageContextHolder
        {
            public IMessageContext? Context;
        }
    }
}
