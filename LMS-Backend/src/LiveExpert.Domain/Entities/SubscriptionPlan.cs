using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

// Feature 10: Subscription plans
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int HoursIncluded { get; set; }
    /// <summary>Maximum sessions per billing cycle (0 = unlimited)</summary>
    public int SessionsLimit { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<StudentSubscription> Subscriptions { get; set; } = new List<StudentSubscription>();
}

public class StudentSubscription : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int HoursUsed { get; set; } = 0;
    public int HoursRemaining { get; set; }
    /// <summary>Sessions booked in this billing cycle (for usage tracking)</summary>
    public int SessionsUsed { get; set; } = 0;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public string? GatewayOrderId { get; set; }
    public Guid? PaymentId { get; set; }

    // Auto-Renewal (Feature 21)
    public bool AutoRenew { get; set; } = false;
    public DateTime? RenewalReminderSentAt { get; set; }

    // Cancellation Retention (Feature 24)
    public string? CancellationReason { get; set; }
    public bool RetentionDiscountOffered { get; set; } = false;
    public decimal RetentionDiscountPercent { get; set; } = 0;
    public DateTime? RetentionOfferExpiry { get; set; }
    /// <summary>Waiting for student to accept or reject retention discount before final cancellation</summary>
    public bool PendingCancellation { get; set; } = false;

    // Navigation
    public User Student { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
    public Payment? Payment { get; set; }
}
