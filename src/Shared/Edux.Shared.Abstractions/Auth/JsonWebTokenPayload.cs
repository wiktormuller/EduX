namespace Edux.Shared.Abstractions.Auth
{
    public class JsonWebTokenPayload
    {
        public string Subject { get; }
        public string Role { get; }
        public long Expires { get; }
        public IDictionary<string, IEnumerable<string>> Claims { get; }

        public JsonWebTokenPayload(string subject, string role, long expires, 
            IDictionary<string, IEnumerable<string>> claims)
        {
            Subject = subject;
            Role = role;
            Expires = expires;
            Claims = claims;
        }
    }
}
