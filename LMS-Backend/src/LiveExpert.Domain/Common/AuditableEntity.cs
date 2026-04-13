namespace LiveExpert.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}
