using System.Text.Json.Serialization;

namespace SmartKey.Application.Common.Cache
{
    public class OtpAttemptCacheModel : CacheModelBase
    {
        public int AttemptCount { get; set; }
        public DateTime NextAvailableAt { get; set; }

        [JsonConstructor]
        public OtpAttemptCacheModel() : base()
        {
        }
        public OtpAttemptCacheModel(int attemptCount, DateTime nextAvailableAt, string cacheKey, TimeSpan absoluteExpiresAt)
            : base($"otp-attempt:{cacheKey}", absoluteExpiresAt)
        {
            AttemptCount = attemptCount;
            NextAvailableAt = nextAvailableAt;
        }

        public void Increase() => AttemptCount++;

        public void SetCooldown(TimeSpan cooldown)
        {
            NextAvailableAt = DateTime.UtcNow.Add(cooldown);
        }
    }
}
