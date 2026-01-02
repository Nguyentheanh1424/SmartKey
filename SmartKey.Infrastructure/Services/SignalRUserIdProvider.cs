using Microsoft.AspNetCore.SignalR;
using SmartKey.Application.Common.Interfaces.Services;

namespace SmartKey.API.SignalR
{
    public class SignalRUserIdProvider : IUserIdProvider
    {
        private readonly ISignalRUserIdProvider _provider;

        public SignalRUserIdProvider(ISignalRUserIdProvider provider)
        {
            _provider = provider;
        }

        public string? GetUserId(HubConnectionContext connection)
        {
            return _provider
                .GetUserId(connection.User)
                ?.ToLower();
        }
    }
}
