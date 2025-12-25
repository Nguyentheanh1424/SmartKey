using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorSharesFeatures.Dtos
{
    public class DoorShareDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public DoorPermission Permission { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
