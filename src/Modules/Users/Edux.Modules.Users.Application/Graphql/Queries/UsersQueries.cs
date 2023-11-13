using Edux.Modules.Users.Application.Graphql.Messaging;
using Edux.Modules.Users.Application.Graphql.Types;
using Edux.Modules.Users.Application.Queries;
using Edux.Shared.Abstractions.Queries;
using GraphQL.Types;
using System.Security.Claims;

namespace Edux.Modules.Users.Application.Graphql.Queries
{
    public class UsersQueries : ObjectGraphType
    {
        public UsersQueries(IQueryDispatcher queryDispatcher, 
            UsersMessageService usersMessageService)
        {
            Field<UserMeType>(name: "UserMe")
                .ResolveAsync(async context =>
                {
                    var user = (ClaimsPrincipal)context.UserContext;
                    var isUserAuthenticated = ((ClaimsIdentity)user.Identity).IsAuthenticated;

                    if (isUserAuthenticated)
                    {
                        throw new UnauthorizedAccessException();
                    }

                    var userId = Guid.Parse(((ClaimsIdentity)user.Identity).Name);

                    var userMeResponse = await queryDispatcher.QueryAsync(new GetUserMe(userId));

                    usersMessageService.AddReturnedUserMeMessage(userMeResponse!);

                    return userMeResponse;
                });
        }
    }
}
