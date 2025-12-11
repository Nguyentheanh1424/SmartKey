namespace SmartKey.Application.Common.Interfaces.Auth
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Provider { get; }

        // Additional properties can be added as needed
    }
}
