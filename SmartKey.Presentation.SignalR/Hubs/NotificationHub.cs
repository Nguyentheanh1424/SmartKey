using Microsoft.AspNetCore.SignalR;

namespace SmartKey.Presentation.SignalR.Hubs
{
    public class NotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"[HUB] Connected: {Context.UserIdentifier}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[HUB] Disconnected: {Context.UserIdentifier}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
