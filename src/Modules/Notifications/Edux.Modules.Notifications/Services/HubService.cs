using Edux.Modules.Notifications.Dto;
using Edux.Modules.Notifications.Hubs;

namespace Edux.Modules.Notifications.Services
{
    public sealed class HubService : IHubService
    {
        private readonly IHubWrapper _hubWrapper;

        public HubService(IHubWrapper hubWrapper)
        {
            _hubWrapper = hubWrapper;
        }

        public async Task PublishUserSignedUpAsync(UserDto user)
        {
            await _hubWrapper.PublishToUserAsync(user.UserId.ToString("N"), "user_signed_up",
                new
                {
                    user.UserId,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                    user.Claims
                });
        }
    }
}
