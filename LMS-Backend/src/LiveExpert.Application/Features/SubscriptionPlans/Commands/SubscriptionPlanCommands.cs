using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.SubscriptionPlans.Commands;

// Feature 10: Subscription plans

public class SubscribeCommand : IRequest<Result<SubscriptionOrderDto>>
{
    public Guid PlanId { get; set; }
}

public class ActivateSubscriptionPlanCommand : IRequest<Result<StudentSubscriptionDto>>
{
    public Guid PlanId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

/// <summary>Feature 24: Cancel subscription with optional reason. Returns retention offer if eligible.</summary>
public class CancelSubscriptionCommand : IRequest<Result<CancelSubscriptionResult>>
{
    public string? Reason { get; set; }
    /// <summary>When true, skips retention offer and cancels immediately.</summary>
    public bool ForceCancel { get; set; } = false;
}

/// <summary>Feature 24: Student accepts the retention discount offer — creates discounted renewal order.</summary>
public class AcceptRetentionOfferCommand : IRequest<Result<SubscriptionOrderDto>>
{
}

/// <summary>Feature 24: Student explicitly rejects retention offer — proceeds with cancellation.</summary>
public class RejectRetentionOfferCommand : IRequest<Result>
{
}

/// <summary>Feature 21: Toggle auto-renewal on the active subscription.</summary>
public class SetAutoRenewalCommand : IRequest<Result>
{
    public bool AutoRenew { get; set; }
}

/// <summary>Feature 25: Switch to a different plan with proration.</summary>
public class SwitchSubscriptionPlanCommand : IRequest<Result<SwitchPlanResult>>
{
    public Guid NewPlanId { get; set; }
}

/// <summary>Feature 25: Activate the prorated upgrade order after Razorpay payment.</summary>
public class ActivatePlanSwitchCommand : IRequest<Result<StudentSubscriptionDto>>
{
    public Guid NewPlanId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class GetSubscriptionPlansQuery : IRequest<Result<List<SubscriptionPlanDto>>>
{
}

public class GetMySubscriptionQuery : IRequest<Result<StudentSubscriptionDto?>>
{
}

// ─── DTOs ───────────────────────────────────────────────────────────────────

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int HoursIncluded { get; set; }
    public int SessionsLimit { get; set; }
    public bool IsActive { get; set; }
}

public class SubscriptionOrderDto
{
    public Guid SubscriptionId { get; set; }
    public string GatewayOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string RazorpayKey { get; set; } = string.Empty;
}

public class StudentSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int HoursUsed { get; set; }
    public int HoursRemaining { get; set; }
    /// <summary>Feature 23: Sessions booked this cycle</summary>
    public int SessionsUsed { get; set; }
    /// <summary>Feature 23: Plan's session cap (0 = unlimited)</summary>
    public int SessionsLimit { get; set; }
    /// <summary>Feature 23: 0-100 usage percentage of sessions</summary>
    public int UsagePercent { get; set; }
    public SubscriptionStatus Status { get; set; }
    public bool AutoRenew { get; set; }
    public int DaysRemaining { get; set; }
}

public class CancelSubscriptionResult
{
    public bool Cancelled { get; set; }
    public bool RetentionOfferMade { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SwitchPlanResult
{
    /// <summary>true = downgrade, credit given as bonus points, switch is immediate</summary>
    public bool ImmediateSwitch { get; set; }
    /// <summary>true = upgrade, Razorpay order created for prorated difference</summary>
    public bool RequiresPayment { get; set; }
    public Guid? NewSubscriptionId { get; set; }
    public string? GatewayOrderId { get; set; }
    public decimal? ChargeAmount { get; set; }
    public decimal? CreditAmount { get; set; }
    public string RazorpayKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
