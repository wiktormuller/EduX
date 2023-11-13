using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Edux.Modules.Users.Application.Graphql.Contexts
{
    public class UserContext : Dictionary<string, object?>
    {
        public ClaimsPrincipal User { get; }

        public UserContext(HttpContext context)
        {
            User = context.User;
        }
    }
}
