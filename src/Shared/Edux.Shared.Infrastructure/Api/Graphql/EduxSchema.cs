using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Api.Graphql
{
    internal sealed class EduxSchema : Schema
    {
        public EduxSchema(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<EduxCompositeQuery>();

            Subscription = serviceProvider.GetRequiredService<EduxCompositeSubscription>();
        }
    }
}
