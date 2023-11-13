using Edux.Modules.Users.Application.Graphql.Messaging;
using GraphQL.Types;

namespace Edux.Modules.Users.Application.Graphql.Types
{
    public class ReturnedUserMeType : ObjectGraphType<ReturnedUserMeMessage>
    {
        public ReturnedUserMeType()
        {
            Field(returnedUserMe => returnedUserMe.Id,
                type: typeof(StringGraphType));

            Field(returnedUserMe => returnedUserMe.Email,
                type: typeof(StringGraphType));
        }
    }
}
