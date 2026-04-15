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
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public string? GatewayOrderId { get; set; }
    public Guid? PaymentId { get; set; }

    // Navigation
    public User Student { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
    public Payment? Payment { get; set; }
}
