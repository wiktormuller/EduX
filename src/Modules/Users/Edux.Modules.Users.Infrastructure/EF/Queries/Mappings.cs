using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Infrastructure.EF.ReadModels;

namespace Edux.Modules.Users.Infrastructure.EF.Queries
{
    internal static class Mappings
    {
        public static UserResponse AsUserResponse(this UserReadModel userReadModel)
        {
            return new()
            {
                Id = userReadModel.Id,
                Email = userReadModel.Email,
                Role = userReadModel.Role,
                IsActive = userReadModel.IsActive,
                CreatedAt = userReadModel.CreatedAt,
                UpdatedAt = userReadModel.UpdatedAt,
                Claims = userReadModel.Claims
            };
        }

        public static UserMeResponse AsUserMeResponse(this UserReadModel userReadModel)
        {
            return new UserMeResponse()
            {
                Id = userReadModel.Id,
                Email = userReadModel.Email,
                Role = userReadModel.Role,
                IsActive = userReadModel.IsActive,
                CreatedAt = userReadModel.CreatedAt,
                UpdatedAt = userReadModel.UpdatedAt,
                Claims = userReadModel.Claims
            };
        }
    }
}
