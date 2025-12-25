using SmartKey.Domain.Common;
using SmartKey.Domain.Enums;

namespace SmartKey.Domain.Entities
{
    public class User : Entity<Guid>
    {
        private const int NameChangeCooldownDays = 1836;

        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string AvatarUrl { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public AccountRole Role { get; private set; } = AccountRole.User;
        public DateTime? DateOfBirth { get; private set; }
        public DateTime? NameUpdatedAt { get; private set; }

        protected User() { }

        public User(string name, string email, string avatarUrl)
        {
            Name = name;
            Email = email;
            AvatarUrl = avatarUrl;
        }


        public (bool IsSuccess, int RemainingDays) UpdateName(string name)
        {
            if (NameUpdatedAt.HasValue)
            {
                var daysSinceLastUpdate =
                    (DateTime.UtcNow - NameUpdatedAt.Value).TotalDays;

                if (daysSinceLastUpdate <= NameChangeCooldownDays)
                {
                    var remainingDays =
                        (int)Math.Ceiling(NameChangeCooldownDays - daysSinceLastUpdate);

                    return (false, remainingDays);
                }
            }

            Name = name;
            NameUpdatedAt = DateTime.UtcNow;
            return (true, 0);
        }

        public void UpdateAvatar(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
        }

        public void UpdatePhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void UpdateDateOfBirth(DateTime dateOfBirth)
        {
            DateOfBirth = dateOfBirth.Date;
        }

        public void SetRole(AccountRole role)
        {
            Role = role;
        }
    }
}
