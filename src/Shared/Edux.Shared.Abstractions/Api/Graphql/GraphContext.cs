using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Edux.Shared.Abstractions.Api.Graphql
{
    public sealed class GraphContext : Dictionary<string, object?>
    {
        public ClaimsPrincipal User { get; }

        public GraphContext(HttpContext context)
        {
            User = context.User;
        }
    }
}
