using SmartKey.Domain.Common;
using SmartKey.Domain.Enums;

namespace SmartKey.Domain.Entities
{
    public class Passcode : Entity<Guid>
    {
        public Guid DoorId { get; private set; }
        public string Code { get; private set; } = string.Empty;

        public PasscodeType Type { get; private set; }
        public DateTime? ValidFrom { get; private set; }
        public DateTime? ValidTo { get; private set; }

        public bool IsActive { get; private set; } = true;

        protected Passcode() { }

        public Passcode(Guid doorId, string code, PasscodeType type)
        {
            DoorId = doorId;
            Code = code;
            Type = type;
        }

        public void Expire()
        {
            IsActive = false;
        }

        public void SetValidity(DateTime? from, DateTime? to)
        {
            ValidFrom = from;
            ValidTo = to;
        }
    }
}
