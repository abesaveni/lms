using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

// Feature 9: Session bundles
public class SessionBundle : BaseEntity
{
    public Guid TutorId { get; set; }
    public Guid? SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SessionCount { get; set; }
    public decimal TotalPrice { get; set; }
    public int ValidityDays { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal DiscountPercentage { get; set; }

    // Navigation
    public User Tutor { get; set; } = null!;
    public Subject? Subject { get; set; }
    public ICollection<BundlePurchase> Purchases { get; set; } = new List<BundlePurchase>();
}

public class BundlePurchase : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid BundleId { get; set; }
    public int SessionsRemaining { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? PaymentId { get; set; }
    public BundleStatus Status { get; set; } = BundleStatus.Active;
    public string? GatewayOrderId { get; set; }

    // Navigation
    public User Student { get; set; } = null!;
    public SessionBundle Bundle { get; set; } = null!;
    public Payment? Payment { get; set; }
}
