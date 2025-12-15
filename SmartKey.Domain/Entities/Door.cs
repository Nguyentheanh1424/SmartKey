using SmartKey.Domain.Common;

namespace SmartKey.Domain.Entities
{
    public class Door : Entity<Guid>
    {
        public Guid OwnerId { get; private set; }

        public string DoorCode { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;

        public string MqttTopicPrefix { get; private set; } = string.Empty;
        public string MacAddress { get; private set; } = string.Empty;

        public int Battery { get; private set; }
        public DateTime LastSyncAt { get; private set; }

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
    }
}
