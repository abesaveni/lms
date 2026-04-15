using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.SubscriptionPlans.Commands;

// Feature 10: Subscription plans (LMS subscription plans, separate from the basic IsSubscribed flag)

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

public class CancelSubscriptionCommand : IRequest<Result>
{
}

public class GetSubscriptionPlansQuery : IRequest<Result<List<SubscriptionPlanDto>>>
{
}

public class GetMySubscriptionQuery : IRequest<Result<StudentSubscriptionDto?>>
{
}

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int HoursIncluded { get; set; }
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
    public SubscriptionStatus Status { get; set; }
}
