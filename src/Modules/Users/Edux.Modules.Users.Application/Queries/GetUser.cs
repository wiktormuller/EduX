using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Shared.Abstractions.Queries;

namespace Edux.Modules.Users.Application.Queries
{
    public class GetUser : IQuery<UserMeResponse>
    {
        public Guid UserId { get; }

        public GetUser(Guid userId)
        {
            UserId = userId;
        }
    }
}
