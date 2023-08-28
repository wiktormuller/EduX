namespace Edux.Shared.Infrastructure.Dates
{
    internal static class Extensions
    {
        public static long ToUnixTimeStamp(this DateTime dateTime) 
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }
    }
}
