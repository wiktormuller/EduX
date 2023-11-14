using Edux.Modules.Users.Application.Graphql.Messaging;
using Edux.Modules.Users.Application.Graphql.Types;
using Edux.Shared.Abstractions.Api.Graphql;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace Edux.Modules.Users.Application.Graphql.Subscriptions
{
    public class UsersSubscriptions : ObjectGraphType, IGraphQlModuleSubscription
    {
        public UsersSubscriptions(IUsersMessageService usersMessageService)
        {
            AddField(new FieldType
            {
                Name = "returnedUserMe", // For 'returnedUserMe' we will return ReturnedUserMeMessage
                Type = typeof(ReturnedUserMeMessageType),
                StreamResolver = new SourceStreamResolver<ReturnedUserMeMessage>(_ => usersMessageService.GetMessages())
            });
        }
    }
}