namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAsync(string key, int length = 6);
        Task ValidateOtpRequestAsync(string key);
        Task MarkOtpSentAsync(string key);
        Task<bool> VerifyAsync(string key, string otp);
    }
}
