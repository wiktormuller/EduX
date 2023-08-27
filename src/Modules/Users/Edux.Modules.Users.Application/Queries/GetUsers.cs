using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Shared.Abstractions.Queries;

namespace Edux.Modules.Users.Application.Queries
{
    public class GetUsers : IQuery<IEnumerable<UserResponse>>
    {
    }
}
