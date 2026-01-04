using SmartKey.Application.Common.Mappings;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorFeatures.Dtos
{
    public class DoorDetailDto : IMapFrom<Door>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DoorCode { get; set; } = string.Empty;

        public int Battery { get; set; }
        public DateTime LastSyncAt { get; set; }
        public string State { get; set; } = string.Empty;

        public DoorPermission Permission { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
