using SmartKey.Domain.Common;
using SmartKey.Domain.Enums;

namespace SmartKey.Domain.Entities
{
    public class DoorShare : Entity<Guid>
    {
        public Guid DoorId { get; private set; }
        public Guid UserId { get; private set; }

        public DoorPermission Permission { get; private set; }

        public DateTime? ValidFrom { get; private set; }
        public DateTime? ValidTo { get; private set; }

        protected DoorShare() { }

        public DoorShare(
            Guid doorId,
            Guid userId,
            DoorPermission permission,
            DateTime? validFrom = null,
            DateTime? validTo = null)
        {
            DoorId = doorId;
            UserId = userId;
            Permission = permission;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public bool IsActive()
        {
            var now = DateTime.UtcNow;

            if (ValidFrom.HasValue && now < ValidFrom.Value)
                return false;

            if (ValidTo.HasValue && now > ValidTo.Value)
                return false;

            return true;
        }
    }
}
