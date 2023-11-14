using Edux.Shared.Abstractions.Api.Graphql;
using GraphQL.Types;

namespace Edux.Shared.Infrastructure.Api.Graphql
{
    internal sealed class EduxCompositeQuery : ObjectGraphType
    {
        public EduxCompositeQuery(IEnumerable<IGraphQlModuleQuery> graphQlModuleQueries)
        {
            Name = "EduxCompositeQuery";

            foreach (var graphQlModuleQuery in graphQlModuleQueries)
            {
                if (graphQlModuleQuery is not ObjectGraphType graphQlQuery)
                {
                    continue;
                }

                foreach (var field in graphQlQuery.Fields)
                {
                    AddField(field);
                }
            }
        }
    }
}
