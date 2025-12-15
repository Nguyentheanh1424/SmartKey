using SmartKey.Domain.Common;

namespace SmartKey.Domain.Entities
{
    public class ICCard : Entity<Guid>
    {
        public Guid DoorId { get; private set; }
        public string CardUid { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;

        public bool IsActive { get; private set; } = true;

        protected ICCard() { }

        public ICCard(Guid doorId, string uid, string name)
        {
            DoorId = doorId;
            CardUid = uid;
            Name = name;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
