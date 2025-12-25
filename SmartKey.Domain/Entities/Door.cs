using SmartKey.Domain.Common;
using SmartKey.Domain.Enums;

namespace SmartKey.Domain.Entities
{
    public class Door : Entity<Guid>
    {
        public Guid OwnerId { get; private set; }

        public string DoorCode { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;

        public DoorState State { get; private set; } = DoorState.Unknown;

        public string MqttTopicPrefix { get; private set; } = string.Empty;
        public string MacAddress { get; private set; } = string.Empty;

        public int Battery { get; private set; }
        public DateTime LastSyncAt { get; private set; }
        public DateTime? LastSyncRequestedAt { get; private set; }

        protected Door() { }

        public Door(Guid ownerId, string doorCode, string name, string mqttPrefix)
        {
            OwnerId = ownerId;
            DoorCode = doorCode;
            Name = name;
            MqttTopicPrefix = mqttPrefix;
            LastSyncAt = DateTime.UtcNow;
        }

        public void Rename(string name)
        {
            Name = name;
        }

        public void UpdateBattery(int battery)
        {
            Battery = battery;
            LastSyncAt = DateTime.UtcNow;
        }

        public void MarkSyncRequested()
        {
            LastSyncRequestedAt = DateTime.UtcNow;
        }

        public void UpdateState(DoorState state)
        {
            State = state;
            LastSyncAt = DateTime.UtcNow;
        }
    }
}
