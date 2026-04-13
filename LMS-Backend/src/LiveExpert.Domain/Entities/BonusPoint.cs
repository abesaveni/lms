using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class BonusPoint : BaseEntity
{
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public BonusPointReason Reason { get; set; }
    public Guid? ReferenceId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
