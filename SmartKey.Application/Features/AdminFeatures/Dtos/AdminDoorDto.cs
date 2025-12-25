using SmartKey.Application.Common.Mappings;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.AdminFeatures.Dtos
{
    public class AdminDoorDto : IMapFrom<Door>
    {
        public Guid Id { get; set; }
        public string DoorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }

        public string State { get; set; } = string.Empty;

        public int Battery { get; set; }
        public DateTime LastSyncAt { get; set; }

        public string MqttTopicPrefix { get; set; } = string.Empty;
    }
}
