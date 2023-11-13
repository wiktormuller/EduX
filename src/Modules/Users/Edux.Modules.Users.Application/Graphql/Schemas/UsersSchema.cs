using Edux.Modules.Users.Application.Graphql.Queries;
using Edux.Modules.Users.Application.Graphql.Subscriptions;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Modules.Users.Application.Graphql.Schemas
{
    public class UsersSchema : Schema
    {
        public UsersSchema(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<UsersQueries>();

            Subscription = serviceProvider.GetRequiredService<UsersSubscriptions>();
        }
    }
}
