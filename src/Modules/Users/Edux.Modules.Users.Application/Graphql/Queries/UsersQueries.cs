using Edux.Modules.Users.Application.Graphql.Messaging;
using Edux.Modules.Users.Application.Graphql.Types;
using Edux.Modules.Users.Application.Queries;
using Edux.Shared.Abstractions.Api.Graphql;
using Edux.Shared.Abstractions.Queries;
using GraphQL.Types;

namespace Edux.Modules.Users.Application.Graphql.Queries
{
    public class UsersQueries : ObjectGraphType, IGraphQlModuleQuery
    {
        public UsersQueries(IQueryDispatcher queryDispatcher, 
            IUsersMessageService usersMessageService)
        {
            Field<UserMeType>(name: "UserMe")
                .ResolveAsync(async ctx =>
                {
                    var context = ctx.UserContext as GraphContext;
                    var isUserAuthenticated = context?.User?.Identity?.IsAuthenticated?? false;

                    if (isUserAuthenticated)
                    {
                        throw new UnauthorizedAccessException();
                    }

                    var userId = context?.User.Identity.Name;

                    var userMeResponse = await queryDispatcher.QueryAsync(new GetUserMe(Guid.Parse(userId)));

                    usersMessageService.AddReturnedUserMeMessage(userMeResponse!);

                    return userMeResponse;
                });
        }
    }
}
