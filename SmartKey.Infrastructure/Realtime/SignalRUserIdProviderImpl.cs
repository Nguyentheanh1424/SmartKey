using System.Security.Claims;
using SmartKey.Application.Common.Interfaces.Services;

namespace SmartKey.Infrastructure.Services
{
    public class SignalRUserIdProviderImpl : ISignalRUserIdProvider
    {
        public string? GetUserId(ClaimsPrincipal? user)
        {
            return user?
                .FindFirst("userId")
                ?.Value;
        }
    }
}
