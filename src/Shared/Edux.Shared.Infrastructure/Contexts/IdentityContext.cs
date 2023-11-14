using Edux.Shared.Abstractions.Contexts;
using System.Security.Claims;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal sealed class IdentityContext : IIdentityContext
    {
        public bool IsAuthenticated { get; }
        public Guid Id { get; }
        public string Role { get; }
        public Dictionary<string, IEnumerable<string>> Claims { get; }

        public IdentityContext(Guid? id)
        {
            Id = id ?? Guid.Empty;
            IsAuthenticated = id.HasValue;
            Role = string.Empty;
            Claims = new Dictionary<string, IEnumerable<string>>();
        }

        public IdentityContext(ClaimsPrincipal principal)
        {
            if (principal?.Identity is null || string.IsNullOrWhiteSpace(principal.Identity.Name))
            {
                return; // Not every request has Identity
            }

            IsAuthenticated = principal.Identity?.IsAuthenticated is true;
            Id = IsAuthenticated ? Guid.Parse(principal.Identity!.Name) : Guid.Empty;
            Role = principal.Claims.Single(x => x.Type == ClaimTypes.Role).Value;
            Claims = principal.Claims.GroupBy(x => x.Type)
                .ToDictionary(x => x.Key, x => x.Select(c => c.Value.ToString()));
        }

        public bool IsUser() => Role is "user";

        public bool IsAdmin() => Role is "admin";
    }
}
