using Edux.Shared.Abstractions.Api.Graphql;
using GraphQL.Types;

namespace Edux.Shared.Infrastructure.Api.Graphql
{
    internal sealed class EduxCompositeSubscription : ObjectGraphType
    {
        public EduxCompositeSubscription(IEnumerable<IGraphQlModuleSubscription> graphQlModuleSubscriptions)
        {
            Name = "EduxCompositeSubscription";

            foreach (var graphQlModuleSubscription in graphQlModuleSubscriptions)
            {
                if (graphQlModuleSubscription is not ObjectGraphType graphQlSubscription)
                {
                    continue;
                }

                foreach (var field in graphQlSubscription.Fields)
                {
                    AddField(field);
                }
            }
        }
    }
}
