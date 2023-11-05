using Edux.Shared.Abstractions.Auth;
using Microsoft.AspNetCore.SignalR;

namespace Edux.Modules.Notifications.Hubs
{
    public class NotificationsHub : Hub
    {
        private readonly IJwtProvider _jwtProvider;

        public NotificationsHub(IJwtProvider jwtProvider)
        {
            _jwtProvider = jwtProvider;
        }

        public async Task InitializeAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                await DisconnectAsync();
            }

            try
            {
                var payload = _jwtProvider.GetTokenPayload(token);
                if (payload is null)
                {
                    await DisconnectAsync();
                    return;
                }

                var group = Guid.Parse(payload.Subject).ToUserGroup(); // Group per UserId - also can be per TenantId for example
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                await ConnectAsync();
            }
            catch
            {
                await DisconnectAsync();
            }
        }

        private async Task ConnectAsync()
        {
            await Clients.Client(Context.ConnectionId)
                .SendAsync("connected");
        }

        private async Task DisconnectAsync()
        {
            await Clients.Client(Context.ConnectionId)
                .SendAsync("disconnected");
        }
    }
}
