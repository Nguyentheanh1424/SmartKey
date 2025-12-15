using SmartKey.Domain.Common;

namespace SmartKey.Domain.Entities
{
    public class DoorRecord : Entity<Guid>
    {
        public Guid DoorId { get; private set; }
        public DateTime OccurredAt { get; private set; }

        public string Event { get; private set; } = string.Empty;
        public string Method { get; private set; } = string.Empty;
        public string RawPayload { get; private set; } = string.Empty;

        protected DoorRecord() { }

        public DoorRecord(
            Guid doorId,
            string @event,
            string method,
            string rawPayload)
        {
            DoorId = doorId;
            Event = @event;
            Method = method;
            RawPayload = rawPayload;
            OccurredAt = DateTime.UtcNow;
        }
    }
}
