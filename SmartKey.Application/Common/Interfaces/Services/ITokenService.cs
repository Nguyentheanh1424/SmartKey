using SmartKey.Domain.Enums;

namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface ITokenService
    {
        Task<(string accessToken, string refreshToken)> IssueAsync(Guid userId, AccountProvider provider, AccountRole role);
        TokenInfoDto ParseAccessToken(string accessToken);
    }

    public class TokenInfoDto
    {
        public Guid UserId { get; set; }
        public string Provider { get; set; } = default!;
        public bool IsExpired { get; set; }
    }
}
