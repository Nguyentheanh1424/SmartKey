using SmartKey.Domain.Common;

namespace SmartKey.Domain.Entities
{
    public class MqttInboxMessage : Entity<Guid>
    {
        public string Topic { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;

        public string Fingerprint { get; private set; } = string.Empty;

        public Guid? DoorId { get; private set; }

        public bool IsProcessed { get; private set; }
        public DateTime ReceivedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        protected MqttInboxMessage() { }

        public MqttInboxMessage(string topic, string payload, string fingerprint, Guid? doorId)
        {
            Topic = topic;
            Payload = payload;
            Fingerprint = fingerprint;
            DoorId = doorId;
            ReceivedAt = DateTime.UtcNow;
        }

        public void MarkProcessed()
        {
            IsProcessed = true;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
