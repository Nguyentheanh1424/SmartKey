namespace SmartKey.Domain.Common
{
    public interface IHasCreationTime
    {
        DateTime CreatedAt { get; set; }
        Guid? CreatedBy { get; set; }
    }

    public abstract class CreationAuditEntity<TId> : Entity<TId>, IHasCreationTime
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
    }

}
