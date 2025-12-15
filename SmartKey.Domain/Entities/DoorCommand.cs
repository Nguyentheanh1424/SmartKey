using SmartKey.Domain.Common;
using SmartKey.Domain.Enums;

namespace SmartKey.Domain.Entities
{
    public class DoorCommand : Entity<Guid>
    {
        public Guid DoorId { get; private set; }
        public string CommandType { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;

        public CommandStatus Status { get; private set; }
        public DateTime SentAt { get; private set; }
        public DateTime? AckAt { get; private set; }

        protected DoorCommand() { }

        public DoorCommand(Guid doorId, string type, string payload)
        {
            DoorId = doorId;
            CommandType = type;
            Payload = payload;
            Status = CommandStatus.Pending;
            SentAt = DateTime.UtcNow;
        }

        public void MarkSuccess()
        {
            Status = CommandStatus.Success;
            AckAt = DateTime.UtcNow;
        }

        public void MarkFailed()
        {
            Status = CommandStatus.Failed;
        }
    }
}
