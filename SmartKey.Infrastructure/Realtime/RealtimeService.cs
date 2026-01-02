using Microsoft.AspNetCore.SignalR;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Presentation.SignalR.Hubs;

namespace SmartKey.Infrastructure.Services
{
    public class RealtimeService : IRealtimeService
    {
        private readonly IHubContext<NotificationHub> _userHub;

        public RealtimeService(IHubContext<NotificationHub> userHub)
        {
            _userHub = userHub;
        }

        public Task SendNotiToUserAsync(Guid userId, string method, object payload)
        {
            Console.WriteLine($"[REALTIME] Has clients = {_userHub.Clients != null}");
            Console.WriteLine($"[REALTIME] Send to user {userId}, method={method}");
            return _userHub.Clients
                .User(userId.ToString().ToLower())
                .SendAsync(method, payload);
        }
    }
}
