namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface ISignalRUserIdProvider
    {
        string? GetUserId(System.Security.Claims.ClaimsPrincipal? user);
    }
}
