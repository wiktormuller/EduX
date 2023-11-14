using Edux.Modules.Users.Application.Graphql.Messaging;
using GraphQL.Types;

namespace Edux.Modules.Users.Application.Graphql.Types
{
    public class ReturnedUserMeMessageType : ObjectGraphType<ReturnedUserMeMessage>
    {
        public ReturnedUserMeMessageType()
        {
            Field(returnedUserMe => returnedUserMe.Id,
                type: typeof(StringGraphType));

            Field(returnedUserMe => returnedUserMe.Email,
                type: typeof(StringGraphType));
        }
    }
}
