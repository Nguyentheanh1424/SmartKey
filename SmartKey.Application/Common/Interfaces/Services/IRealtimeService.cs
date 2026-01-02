namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface IRealtimeService
    {
        Task SendNotiToUserAsync(Guid userId, string method, object payload);
    }
}
