namespace SmartKey.Domain.Common
{
    public interface IHasModificationTime
    {
        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedBy { get; set; }
    }

    public abstract class ModificationAuditEntity<TId>
        : CreationAuditEntity<TId>, IHasModificationTime
    {
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }

}
