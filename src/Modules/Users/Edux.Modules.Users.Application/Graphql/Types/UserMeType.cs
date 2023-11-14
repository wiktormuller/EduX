using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Shared.Abstractions.Serializers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Modules.Users.Application.Graphql.Types
{
    public class UserMeType : ObjectGraphType<UserMeResponse>
    {
        public UserMeType()
        {
            // We also can use resolvers here to get data from some other source
            Field(type: typeof(StringGraphType), name: "Id")
                .Resolve(context =>
                {
                    return context.Source.Id.ToString("N");
                });

            Field(userResponse => userResponse.Email,
                type: typeof(StringGraphType));

            Field(userResponse => userResponse.Role,
                type: typeof(StringGraphType));

            Field(userResponse => userResponse.IsActive,
                type: typeof(BooleanGraphType));

            Field(userResponse => userResponse.CreatedAt,
                type: typeof(DateTimeGraphType));

            Field(userResponse => userResponse.UpdatedAt,
                type: typeof(DateTimeGraphType));

            Field(type: typeof(StringGraphType), name: "Claims")
                .Resolve(context =>
                {
                    var serializer = context.RequestServices.GetRequiredService<IJsonSerializer>();
                    return serializer.Serialize(context.Source.Claims);
                });
        }
    }
}
