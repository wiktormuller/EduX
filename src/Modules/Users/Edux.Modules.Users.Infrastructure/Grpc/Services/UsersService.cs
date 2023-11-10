using Edux.Modules.Users.Application.Grpc.Protos;
using Edux.Modules.Users.Application.Queries;
using Google.Protobuf.WellKnownTypes;
using Edux.Shared.Abstractions.Queries;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Edux.Shared.Abstractions.Serializers;

namespace Edux.Modules.Users.Infrastructure.Grpc
{
    internal sealed class UsersService : GrpcUsersService.GrpcUsersServiceBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IJsonSerializer _jsonSerializer;

        public UsersService(IQueryDispatcher queryDispatcher,
            IJsonSerializer jsonSerializer)
        {
            _queryDispatcher = queryDispatcher;
            _jsonSerializer = jsonSerializer;
        }

        [Authorize]
        public override async Task<GetUserMeResponse> GetUserMe(GetUserMeRequest request, ServerCallContext context)
        {
            var userId = context?.GetHttpContext()?.User?.Identity?.Name;

            if (userId is null)
            {
                return null;
            }

            var userMe = await _queryDispatcher.QueryAsync(new GetUserMe(Guid.Parse(userId)));

            return new GetUserMeResponse
            {
                Id = userMe.Id.ToString(),
                Email = userMe.Email,
                Role = userMe.Role,
                CreatedAt = Timestamp.FromDateTime(userMe.CreatedAt),
                IsActive = userMe.IsActive,
                UpdatedAt = Timestamp.FromDateTime(userMe.UpdatedAt),
                ClaimsAsJson = _jsonSerializer.Serialize(userMe.Claims)
            };
        }
    }
}
