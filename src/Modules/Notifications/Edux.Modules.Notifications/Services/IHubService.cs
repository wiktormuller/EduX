using Edux.Modules.Notifications.Dto;

namespace Edux.Modules.Notifications.Services
{
    public interface IHubService
    {
        Task PublishUserSignedUpAsync(UserDto user);
    }
}
