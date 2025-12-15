using SmartKey.Application.Common.Cache;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common.Helpers;

namespace SmartKey.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly ICacheService _cache;
        private readonly TimeSpan AttemptTtl = TimeSpan.FromMinutes(10);
        private readonly TimeSpan Cooldown = TimeSpan.FromSeconds(30);
        private const int MaxAttempts = 5;


        public OtpService(ICacheService cache)
        {
            _cache = cache;
        }

        private static string BuildOtpCacheKey(string key)
            => $"otp:{key}";

        private static string BuildOtpAttemptCacheKey(string key)
            => $"otp-attempt:{key}";

        public async Task<string> GenerateAsync(string key, int length = 6)
        {
            //var otp = OtpGenerator.GenerateNumeric(length);
            var otp = OtpGenerator.Generate(length);

            // TTL OTP 2 phút
            var ttl = TimeSpan.FromMinutes(5);

            var model = new OtpCacheModel(key, otp, ttl);

            await _cache.SetAsync(model, absoluteTtl: ttl);

            return otp;
        }

        private async Task<OtpAttemptCacheModel> GetOrCreateAttemptAsync(string key)
        {
            var cacheKey = BuildOtpAttemptCacheKey(key);

            var model = await _cache.GetAsync<OtpAttemptCacheModel>(cacheKey);

            if (model == null)
            {
                return new OtpAttemptCacheModel(
                    cacheKey: key,
                    attemptCount: 0,
                    nextAvailableAt: DateTime.MinValue,
                    absoluteExpiresAt: AttemptTtl
                    );
            }

            return model;
        }

        public async Task ValidateOtpRequestAsync(string key)
        {
            var attempt = await GetOrCreateAttemptAsync(key);

            // Kiểm tra limit
            if (attempt.AttemptCount >= MaxAttempts)
            {
                throw new BusinessException("Bạn đã yêu cầu OTP quá nhiều lần. Hãy thử lại sau 10 phút.");
            }

            // Kiểm tra cooldown
            if (DateTime.UtcNow < attempt.NextAvailableAt)
            {
                var remain = (attempt.NextAvailableAt - DateTime.UtcNow).TotalSeconds;
                throw new BusinessException($"Bạn phải chờ thêm {Math.Ceiling(remain)} giây trước khi gửi OTP tiếp.");
            }
        }

        public async Task MarkOtpSentAsync(string key)
        {
            var attempt = await GetOrCreateAttemptAsync(key);

            // Tăng attempt và set cooldown
            attempt.Increase();
            attempt.SetCooldown(Cooldown);

            await _cache.SetAsync(attempt, absoluteTtl: AttemptTtl);
        }

        public async Task<bool> VerifyAsync(string key, string otp)
        {
            var cacheKey = BuildOtpCacheKey(key);

            var model = await _cache.GetAsync<OtpCacheModel>(cacheKey);
            if (model == null)
                return false;

            if (model.Otp != otp)
                return false;

            // Dùng xong xoá luôn
            await _cache.RemoveAsync(cacheKey);

            return true;
        }

        public async Task ClearAttemptAsync(string key)
        {
            var attemptKey = BuildOtpAttemptCacheKey(key);
            await _cache.RemoveAsync(attemptKey);
        }
    }
}
