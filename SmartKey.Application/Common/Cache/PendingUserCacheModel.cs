namespace SmartKey.Application.Common.Cache
{
    public class PendingUserCacheModel : CacheModelBase
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;

        public PendingUserCacheModel() : base("") { }

        public PendingUserCacheModel(string email, string name, string passwordHash, string salt, TimeSpan ttl)
            : base($"pending-user:{email}", absoluteExpiresAt: ttl)
        {
            Email = email;
            Name = name;
            PasswordHash = passwordHash;
            Salt = salt;
        }
    }
}
