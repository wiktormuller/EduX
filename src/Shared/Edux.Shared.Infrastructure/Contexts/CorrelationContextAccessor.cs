using Edux.Shared.Abstractions.Contexts;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal sealed class CorrelationContextAccessor : ICorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContextHolder> Holder = new();

        public ICorrelationContext CorrelationContext
        {
            get => Holder.Value?.Context;
            set
            {
                var holder = Holder.Value;
                if (holder is not null)
                {
                    holder.Context = null;
                }

                if (value is not null)
                {
                    Holder.Value = new CorrelationContextHolder
                    {
                        Context = value
                    };
                }
            }
        }

        private class CorrelationContextHolder
        {
            public ICorrelationContext Context;
        }
    }
}
