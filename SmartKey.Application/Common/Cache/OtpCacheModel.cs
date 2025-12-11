namespace SmartKey.Application.Common.Cache
{
    public class OtpCacheModel : CacheModelBase
    {
        public string Otp { get; set; } = string.Empty;

        public OtpCacheModel() : base("") { }

        public OtpCacheModel(string key, string otp, TimeSpan ttl)
        : base($"otp:{key}", absoluteExpiresAt: ttl)
        {
            Otp = otp;
        }
    }
}
