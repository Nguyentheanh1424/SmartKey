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
            ValidatePermissionWithTime(permission, validFrom, validTo);

            DoorId = doorId;
            UserId = userId;
            Permission = permission;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public void UpdatePermission(
            Guid actorUserId,
            DoorActorRole actorRole,
            DoorPermission newPermission,
            DateTime? validFrom,
            DateTime? validTo)
        {
            if (actorUserId == UserId)
                throw new DomainException(
                    "Không thể tự chỉnh quyền chia sẻ của chính mình.");

            if (actorRole == DoorActorRole.Admin &&
                newPermission == DoorPermission.Admin)
            {
                throw new DomainException(
                    "Admin không thể gán hoặc chỉnh quyền Admin.");
            }

            ValidatePermissionWithTime(newPermission, validFrom, validTo);

            Permission = newPermission;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        private static void ValidatePermissionWithTime(
            DoorPermission permission,
            DateTime? validFrom,
            DateTime? validTo)
        {
            if (permission != DoorPermission.TimeRestricted &&
                (validFrom.HasValue || validTo.HasValue))
            {
                throw new DomainException(
                    "Chỉ quyền TimeRestricted mới được phép giới hạn thời gian.");
            }

            if (permission == DoorPermission.TimeRestricted &&
                !validFrom.HasValue && !validTo.HasValue)
            {
                throw new DomainException(
                    "Quyền TimeRestricted phải có thời gian bắt đầu hoặc kết thúc.");
            }

            if (validFrom.HasValue && validTo.HasValue &&
                validFrom > validTo)
            {
                throw new DomainException(
                    "Thời gian truy cập không hợp lệ.");
            }
        }

        public (bool isValid, string message) IsActive()
        {
            var now = DateTime.UtcNow;

            if (Permission == DoorPermission.Admin || Permission == DoorPermission.User)
                return (true, "Quyền truy cập hợp lệ.");

            if (Permission == DoorPermission.TimeRestricted)
            {
                if (ValidFrom.HasValue && now < ValidFrom.Value)
                    return (false, "Quyền truy cập chưa có hiệu lực.");

                if (ValidTo.HasValue && now > ValidTo.Value)
                    return (false, "Quyền truy cập đã hết hạn.");
            }

            return (false, "Quyền truy không xác định.");
        }
    }
}
