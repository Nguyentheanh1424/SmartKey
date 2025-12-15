using SmartKey.Domain.Common;

namespace SmartKey.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string AvatarUrl { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public DateTime? DateOfBirth { get; private set; }

        public IReadOnlyCollection<Guid> OwnedDoorIds => _ownedDoorIds;
        private readonly List<Guid> _ownedDoorIds = new();

        protected User() { }

        public User(string name, string email, string avatarUrl)
        {
            Name = name;
            Email = email;
            AvatarUrl = avatarUrl;
        }


        public void UpdateName(string name)
        {
            Name = name;
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

        public void AddOwnedDoor(Guid doorId)
        {
            if (!_ownedDoorIds.Contains(doorId))
                _ownedDoorIds.Add(doorId);
        }
    }
}
