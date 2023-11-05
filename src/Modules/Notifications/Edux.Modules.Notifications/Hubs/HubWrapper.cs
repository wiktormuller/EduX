using Microsoft.AspNetCore.SignalR;

namespace Edux.Modules.Notifications.Hubs
{
    public class HubWrapper : IHubWrapper
    {
        private readonly IHubContext<NotificationsHub> _hubContext;

        public HubWrapper(IHubContext<NotificationsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishToAllAsync(string message, object data)
        {
            await _hubContext.Clients.All.SendAsync(message, data);
        }

        public async Task PublishToUserAsync(string userId, string message, object data)
        {
            await _hubContext.Clients.Group(userId.ToUserGroup()).SendAsync(message, data);
        }
    }
}
