using SmartKey.Domain.Common;
using SmartKey.Domain.Enums;

namespace SmartKey.Domain.Entities
{
    public class UserAuth : FullAuditEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        public AuthStatus Status { get; private set; } = AuthStatus.Active;

        public string Provider { get; private set; } = EnumExtensions.GetName(AccountProvider.Local);
        public string ProviderUid { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;
        public string Salt { get; private set; } = string.Empty;

        public string RefreshToken { get; private set; } = string.Empty;
        public DateTime? RefreshTokenExpireAt { get; private set; }

        public int LoginAttempt { get; private set; } = 0;
        public DateTime LastPasswordChangeAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LockedUntil { get; private set; }
        public DateTime LastLoginAt { get; private set; } = DateTime.UtcNow;

        protected UserAuth() { }

        public static UserAuth CreateLocal(User user, string passwordHash, string salt)
        {
            return new UserAuth
            {
                User = user,
                UserId = user.Id,
                PasswordHash = passwordHash,
                Salt = salt,
            };
        }

        public static UserAuth CreateOAuth(User user, string provider, string providerUid)
        {
            return new UserAuth
            {
                User = user,
                UserId = user.Id,
                Provider = provider,
                ProviderUid = providerUid,
            };
        }

        public void SetPassword(string hash, string salt)
        {
            PasswordHash = hash;
            Salt = salt;
        }

        public void SetRefreshToken(string refreshToken, int days)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpireAt = DateTime.UtcNow.AddDays(days);
        }

        public void RevokeRefreshToken()
        {
            RefreshToken = string.Empty;
            RefreshTokenExpireAt = null;
        }

        public void MarkLoginSuccess()
        {
            LoginAttempt = 0;
            LockedUntil = null;
            Status = AuthStatus.Active;
            LastLoginAt = DateTime.UtcNow;
        }

        public string MarkLoginFailed(int maxFail = 5, int lockMinutes = 30)
        {
            LoginAttempt++;

            if (LoginAttempt >= maxFail)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(lockMinutes);
                Status = AuthStatus.Locked;
                LoginAttempt = 0;
                return $"Tài khoản bị khóa {lockMinutes} phút.";
            }


            return $"Còn {maxFail - LoginAttempt} lần thử.";
        }

        public (bool isLocked, string? remaining) IsLocked()
        {
            if (!LockedUntil.HasValue)
                return (false, null);

            bool locked = DateTime.UtcNow < LockedUntil.Value;
            TimeSpan? remaining = locked ? LockedUntil.Value - DateTime.UtcNow : null;

            string? formatted = remaining?.ToString(@"mm\:ss");

            return (locked, formatted);
        }

        public void Lock()
        {
            Status = AuthStatus.Locked;
        }

        public void Disable()
        {
            Status = AuthStatus.Disabled;
        }

        public void Ban()
        {
            Status = AuthStatus.Banned;
        }

        public void Activate()
        {
            Status = AuthStatus.Active;
            LockedUntil = null;
            LoginAttempt = 0;
        }

        public void Delete()
        {
            Status = AuthStatus.Deleted;
        }

        public (bool isUsable, string message) IsActive()
        {
            bool isUsable = true;
            string message = string.Empty;

            switch (Status)
            {
                case AuthStatus.Active:
                    message = "Tài khoản đã được kích hoạt.";
                    break;

                case AuthStatus.Locked:
                    isUsable = false;
                    message = "Tài khoản đã bị khóa, vui lòng liên hệ quản trị viên qua Zalo theo số liên hệ 0966963030 để kích hoạt lại tài khoản.";
                    break;

                case AuthStatus.Banned:
                    isUsable = false;
                    message = "Tài khoản đã bị khóa vĩnh viễn.";
                    break;

                case AuthStatus.Deleted:
                    isUsable = false;
                    message = "Tài khoản đã bị xóa.";
                    break;

                default:
                    isUsable = false;
                    message = "Trạng thái tài khoản không hợp lệ.";
                    break;
            }

            return (isUsable, message);
        }

        public bool IsRefreshTokenExpired()
        {
            if (!RefreshTokenExpireAt.HasValue)
                return true;

            return RefreshTokenExpireAt.Value < DateTime.UtcNow;
        }

        public (bool isValidate, string message) ValidateRefreshToken(string refreshToken)
        {
            if (RefreshToken != refreshToken)
                return (false, "Refresh token không hợp lệ.");

            if (!IsRefreshTokenExpired())
            {
                return (true, "Token hợp lệ");
            }
            else
            {
                return (false, "Refresh token đã hết hạn");
            }
        }

    }
}
