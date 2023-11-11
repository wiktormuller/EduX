using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Infrastructure.EF.ReadModels;

namespace Edux.Modules.Users.Infrastructure.EF.Queries
{
    internal static class Mappings
    {
        public static UserResponse AsUserResponse(this UserReadModel userReadModel)
        {
            return new
            (
                userReadModel.Id,
                userReadModel.Email,
                userReadModel.Role,
                userReadModel.IsActive,
                userReadModel.CreatedAt,
                userReadModel.UpdatedAt,
                userReadModel.Claims
            );
        }

        public static UserDetailsResponse AsUserDetailsResponse(this UserReadModel userReadModel)
        {
            return new
            (
                userReadModel.Id,
                userReadModel.Email,
                userReadModel.Role,
                userReadModel.IsActive,
                userReadModel.CreatedAt,
                userReadModel.UpdatedAt,
                userReadModel.Claims
            );
        }

        public static UserMeResponse AsUserMeResponse(this UserReadModel userReadModel)
        {
            return new UserMeResponse
            (
                userReadModel.Id,
                userReadModel.Email,
                userReadModel.Role,
                userReadModel.IsActive,
                userReadModel.CreatedAt,
                userReadModel.UpdatedAt,
                userReadModel.Claims
            );
        }
    }
}
