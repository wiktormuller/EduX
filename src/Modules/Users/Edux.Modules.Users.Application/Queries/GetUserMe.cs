using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Shared.Abstractions.Queries;

namespace Edux.Modules.Users.Application.Queries
{
    public class GetUserMe : IQuery<UserMeResponse?>
    {
        public Guid UserId { get; }

        public GetUserMe(Guid userId)
        {
            UserId = userId;
        }
    }
}
