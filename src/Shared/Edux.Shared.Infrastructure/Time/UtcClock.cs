using Edux.Shared.Abstractions.Time;

namespace Edux.Shared.Infrastructure.Time
{
    internal sealed class UtcClock : IClock
    {
        public DateTime CurrentDate()
        {
            return DateTime.UtcNow;
        }
    }
}
