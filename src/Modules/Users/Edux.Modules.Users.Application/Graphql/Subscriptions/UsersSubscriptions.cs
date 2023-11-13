using Edux.Modules.Users.Application.Graphql.Messaging;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace Edux.Modules.Users.Application.Graphql.Subscriptions
{
    public class UsersSubscriptions : ObjectGraphType
    {
        public UsersSubscriptions(UsersMessageService usersMessageService)
        {
            Name = "Subsription";

            AddField(new FieldType
            {
                Name = "returnedUserMe", // For 'returnedUserMe' we will return ReturnedUserMeMessage
                Type = typeof(ReturnedUserMeMessage),
                StreamResolver = new SourceStreamResolver<ReturnedUserMeMessage>(_ => usersMessageService.GetMessages())
            });
        }
    }
}