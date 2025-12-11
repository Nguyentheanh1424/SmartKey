namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface IOAuthVerifier
    {
        Task<OAuthProfileDto> VerifyGoogleAsync(string token);
        Task<OAuthProfileDto> VerifyFacebookAsync(string token);
    }

    public class OAuthProfileDto
    {
        public string Uid { get; set; } = default!;
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}
