using Microsoft.AspNetCore.SignalR;

namespace Edux.Modules.Notifications.Hubs
{
    public interface IHubWrapper
    {
        Task PublishToUserAsync(string userId, string message, object data);
        Task PublishToAllAsync(string message, object data);
    }
}
