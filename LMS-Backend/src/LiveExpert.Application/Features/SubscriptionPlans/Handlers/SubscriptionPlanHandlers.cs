using LiveExpert.Application.Common;
using LiveExpert.Application.Features.SubscriptionPlans.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.SubscriptionPlans.Handlers;

// ─── Helpers ────────────────────────────────────────────────────────────────

file static class SubHelper
{
    public static StudentSubscriptionDto ToDto(StudentSubscription sub, SubscriptionPlan plan)
    {
        var daysRemaining = Math.Max(0, (int)(sub.EndDate - DateTime.UtcNow).TotalDays);
        int usagePct = (plan.SessionsLimit > 0)
            ? Math.Min(100, (int)Math.Round((double)sub.SessionsUsed / plan.SessionsLimit * 100))
            : 0;

        return new StudentSubscriptionDto
        {
            Id = sub.Id,
            StudentId = sub.StudentId,
            PlanId = sub.PlanId,
            PlanName = plan.Name,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            HoursUsed = sub.HoursUsed,
            HoursRemaining = sub.HoursRemaining,
            SessionsUsed = sub.SessionsUsed,
            SessionsLimit = plan.SessionsLimit,
            UsagePercent = usagePct,
            Status = sub.Status,
            AutoRenew = sub.AutoRenew,
            DaysRemaining = daysRemaining
        };
    }
}

// ─── Get Plans ───────────────────────────────────────────────────────────────

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly IRepository<SubscriptionPlan> _planRepository;

    public GetSubscriptionPlansQueryHandler(IRepository<SubscriptionPlan> planRepository)
        => _planRepository = planRepository;

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _planRepository.FindAsync(p => p.IsActive, cancellationToken);
        var dtos = plans.OrderBy(p => p.MonthlyPrice).Select(p => new SubscriptionPlanDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            MonthlyPrice = p.MonthlyPrice,
            HoursIncluded = p.HoursIncluded,
            SessionsLimit = p.SessionsLimit,
            IsActive = p.IsActive
        }).ToList();

        return Result<List<SubscriptionPlanDto>>.SuccessResult(dtos);
    }
}

// ─── Get My Subscription ─────────────────────────────────────────────────────

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, Result<StudentSubscriptionDto?>>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMySubscriptionQueryHandler(
        IRepository<StudentSubscription> subRepository,
        IRepository<SubscriptionPlan> planRepository,
        ICurrentUserService currentUserService)
    {
        _subRepository = subRepository;
        _planRepository = planRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StudentSubscriptionDto?>> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentSubscriptionDto?>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var sub = await _subRepository.FirstOrDefaultAsync(
            s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active, cancellationToken);

        if (sub == null)
            return Result<StudentSubscriptionDto?>.SuccessResult(null);

        var plan = await _planRepository.GetByIdAsync(sub.PlanId, cancellationToken);
        if (plan == null)
            return Result<StudentSubscriptionDto?>.SuccessResult(null);

        return Result<StudentSubscriptionDto?>.SuccessResult(SubHelper.ToDto(sub, plan));
    }
}

// ─── Subscribe ───────────────────────────────────────────────────────────────

public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, Result<SubscriptionOrderDto>>
{
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscribeCommandHandler> _logger;

    public SubscribeCommandHandler(
        IRepository<SubscriptionPlan> planRepository,
        IRepository<StudentSubscription> subRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<SubscribeCommandHandler> logger)
    {
        _planRepository = planRepository;
        _subRepository = subRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubscriptionOrderDto>> Handle(SubscribeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<SubscriptionOrderDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
            if (plan == null || !plan.IsActive)
                return Result<SubscriptionOrderDto>.FailureResult("NOT_FOUND", "Subscription plan not found or inactive");

            var existing = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active, cancellationToken);
            if (existing != null)
                return Result<SubscriptionOrderDto>.FailureResult("CONFLICT", "You already have an active subscription");

            var (orderId, key) = await _paymentService.CreateOrderAsync(
                plan.MonthlyPrice, "INR",
                new Dictionary<string, string>
                {
                    { "planId", plan.Id.ToString() },
                    { "studentId", userId.Value.ToString() },
                    { "purpose", "subscription" }
                });

            var now = DateTime.UtcNow;
            var sub = new StudentSubscription
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                PlanId = request.PlanId,
                StartDate = now,
                EndDate = now.AddMonths(1),
                HoursUsed = 0,
                HoursRemaining = plan.HoursIncluded,
                SessionsUsed = 0,
                Status = SubscriptionStatus.Active,
                GatewayOrderId = orderId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _subRepository.AddAsync(sub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<SubscriptionOrderDto>.SuccessResult(new SubscriptionOrderDto
            {
                SubscriptionId = sub.Id,
                GatewayOrderId = orderId,
                Amount = plan.MonthlyPrice,
                Currency = "INR",
                RazorpayKey = key
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating subscription order");
            return Result<SubscriptionOrderDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

// ─── Activate Subscription ───────────────────────────────────────────────────

public class ActivateSubscriptionPlanCommandHandler : IRequestHandler<ActivateSubscriptionPlanCommand, Result<StudentSubscriptionDto>>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateSubscriptionPlanCommandHandler> _logger;

    public ActivateSubscriptionPlanCommandHandler(
        IRepository<StudentSubscription> subRepository,
        IRepository<SubscriptionPlan> planRepository,
        IRepository<Payment> paymentRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<ActivateSubscriptionPlanCommandHandler> logger)
    {
        _subRepository = subRepository;
        _planRepository = planRepository;
        _paymentRepository = paymentRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<StudentSubscriptionDto>> Handle(ActivateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentSubscriptionDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sub = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.PlanId == request.PlanId && s.GatewayOrderId == request.OrderId,
                cancellationToken);

            if (sub == null)
                return Result<StudentSubscriptionDto>.FailureResult("NOT_FOUND", "Subscription record not found");

            var isValid = await _paymentService.VerifyPaymentSignatureAsync(request.OrderId, request.PaymentId, request.Signature);
            if (!isValid)
                return Result<StudentSubscriptionDto>.FailureResult("VALIDATION_ERROR", "Payment signature verification failed");

            var plan = await _planRepository.GetByIdAsync(sub.PlanId, cancellationToken);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                TutorId = Guid.Empty,
                BaseAmount = plan?.MonthlyPrice ?? 0,
                PlatformFee = 0,
                TotalAmount = plan?.MonthlyPrice ?? 0,
                Status = PaymentStatus.Success,
                PaymentGateway = "Razorpay",
                GatewayOrderId = request.OrderId,
                GatewayPaymentId = request.PaymentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment, cancellationToken);

            sub.PaymentId = payment.Id;
            sub.Status = SubscriptionStatus.Active;
            sub.UpdatedAt = DateTime.UtcNow;

            await _subRepository.UpdateAsync(sub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<StudentSubscriptionDto>.SuccessResult(SubHelper.ToDto(sub, plan!));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error activating subscription");
            return Result<StudentSubscriptionDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

// ─── Feature 24: Cancel with Retention Flow ──────────────────────────────────

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result<CancelSubscriptionResult>>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelSubscriptionCommandHandler> _logger;

    public CancelSubscriptionCommandHandler(
        IRepository<StudentSubscription> subRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CancelSubscriptionCommandHandler> logger)
    {
        _subRepository = subRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CancelSubscriptionResult>> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<CancelSubscriptionResult>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sub = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active, cancellationToken);

            if (sub == null)
                return Result<CancelSubscriptionResult>.FailureResult("NOT_FOUND", "No active subscription found");

            // Record cancellation reason
            if (!string.IsNullOrWhiteSpace(request.Reason))
                sub.CancellationReason = request.Reason;

            // Check retention offer eligibility:
            // - First time asking to cancel (no prior offer)
            // - Active for at least 7 days
            // - Not forcing cancel
            var daysActive = (DateTime.UtcNow - sub.StartDate).TotalDays;
            bool offerEligible = !request.ForceCancel
                && !sub.RetentionDiscountOffered
                && daysActive >= 7;

            if (offerEligible)
            {
                sub.RetentionDiscountOffered = true;
                sub.RetentionDiscountPercent = 20m;
                sub.RetentionOfferExpiry = DateTime.UtcNow.AddDays(2);
                sub.PendingCancellation = true;
                sub.UpdatedAt = DateTime.UtcNow;

                await _subRepository.UpdateAsync(sub, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result<CancelSubscriptionResult>.SuccessResult(new CancelSubscriptionResult
                {
                    Cancelled = false,
                    RetentionOfferMade = true,
                    DiscountPercent = 20m,
                    Message = "Before you go — we'd like to offer you 20% off your next month. Accept the offer to renew at a discount, or reject it to cancel."
                });
            }

            // Cancel immediately
            sub.Status = SubscriptionStatus.Cancelled;
            sub.PendingCancellation = false;
            sub.UpdatedAt = DateTime.UtcNow;

            await _subRepository.UpdateAsync(sub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<CancelSubscriptionResult>.SuccessResult(new CancelSubscriptionResult
            {
                Cancelled = true,
                RetentionOfferMade = false,
                DiscountPercent = 0,
                Message = "Subscription cancelled successfully."
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error cancelling subscription");
            return Result<CancelSubscriptionResult>.FailureResult("ERROR", ex.Message);
        }
    }
}

// ─── Feature 24: Accept Retention Offer ─────────────────────────────────────

public class AcceptRetentionOfferCommandHandler : IRequestHandler<AcceptRetentionOfferCommand, Result<SubscriptionOrderDto>>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AcceptRetentionOfferCommandHandler> _logger;

    public AcceptRetentionOfferCommandHandler(
        IRepository<StudentSubscription> subRepository,
        IRepository<SubscriptionPlan> planRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<AcceptRetentionOfferCommandHandler> logger)
    {
        _subRepository = subRepository;
        _planRepository = planRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubscriptionOrderDto>> Handle(AcceptRetentionOfferCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<SubscriptionOrderDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sub = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value
                    && s.Status == SubscriptionStatus.Active
                    && s.PendingCancellation, cancellationToken);

            if (sub == null)
                return Result<SubscriptionOrderDto>.FailureResult("NOT_FOUND", "No pending cancellation with retention offer found");

            if (sub.RetentionOfferExpiry.HasValue && sub.RetentionOfferExpiry.Value < DateTime.UtcNow)
                return Result<SubscriptionOrderDto>.FailureResult("EXPIRED", "Retention offer has expired");

            var plan = await _planRepository.GetByIdAsync(sub.PlanId, cancellationToken);
            if (plan == null)
                return Result<SubscriptionOrderDto>.FailureResult("NOT_FOUND", "Subscription plan not found");

            var discountedPrice = Math.Round(plan.MonthlyPrice * (1 - sub.RetentionDiscountPercent / 100m), 2);

            var (orderId, key) = await _paymentService.CreateOrderAsync(
                discountedPrice, "INR",
                new Dictionary<string, string>
                {
                    { "planId", plan.Id.ToString() },
                    { "studentId", userId.Value.ToString() },
                    { "purpose", "subscription_renewal_retention" }
                });

            // Mark that the offer was accepted — clear pending cancellation
            sub.PendingCancellation = false;
            sub.GatewayOrderId = orderId;
            sub.UpdatedAt = DateTime.UtcNow;

            await _subRepository.UpdateAsync(sub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<SubscriptionOrderDto>.SuccessResult(new SubscriptionOrderDto
            {
                SubscriptionId = sub.Id,
                GatewayOrderId = orderId,
                Amount = discountedPrice,
                Currency = "INR",
                RazorpayKey = key
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error accepting retention offer");
            return Result<SubscriptionOrderDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

// ─── Feature 24: Reject Retention Offer ──────────────────────────────────────

public class RejectRetentionOfferCommandHandler : IRequestHandler<RejectRetentionOfferCommand, Result>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectRetentionOfferCommandHandler(
        IRepository<StudentSubscription> subRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _subRepository = subRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RejectRetentionOfferCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sub = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value
                    && s.Status == SubscriptionStatus.Active
                    && s.PendingCancellation, cancellationToken);

            if (sub == null)
                return Result.FailureResult("NOT_FOUND", "No pending cancellation found");

            sub.Status = SubscriptionStatus.Cancelled;
            sub.PendingCancellation = false;
            sub.UpdatedAt = DateTime.UtcNow;

            await _subRepository.UpdateAsync(sub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.SuccessResult("Subscription cancelled.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

// ─── Feature 21: Set Auto-Renewal ────────────────────────────────────────────

public class SetAutoRenewalCommandHandler : IRequestHandler<SetAutoRenewalCommand, Result>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SetAutoRenewalCommandHandler(
        IRepository<StudentSubscription> subRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _subRepository = subRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetAutoRenewalCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");

        var sub = await _subRepository.FirstOrDefaultAsync(
            s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active, cancellationToken);

        if (sub == null)
            return Result.FailureResult("NOT_FOUND", "No active subscription found");

        sub.AutoRenew = request.AutoRenew;
        sub.UpdatedAt = DateTime.UtcNow;

        await _subRepository.UpdateAsync(sub, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult(request.AutoRenew ? "Auto-renewal enabled." : "Auto-renewal disabled.");
    }
}

// ─── Feature 25: Switch Plan (Upgrade / Downgrade) ───────────────────────────

public class SwitchSubscriptionPlanCommandHandler : IRequestHandler<SwitchSubscriptionPlanCommand, Result<SwitchPlanResult>>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IRepository<BonusPoint> _bonusRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SwitchSubscriptionPlanCommandHandler> _logger;

    public SwitchSubscriptionPlanCommandHandler(
        IRepository<StudentSubscription> subRepository,
        IRepository<SubscriptionPlan> planRepository,
        IRepository<BonusPoint> bonusRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<SwitchSubscriptionPlanCommandHandler> logger)
    {
        _subRepository = subRepository;
        _planRepository = planRepository;
        _bonusRepository = bonusRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SwitchPlanResult>> Handle(SwitchSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<SwitchPlanResult>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sub = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active, cancellationToken);

            if (sub == null)
                return Result<SwitchPlanResult>.FailureResult("NOT_FOUND", "No active subscription found");

            if (sub.PlanId == request.NewPlanId)
                return Result<SwitchPlanResult>.FailureResult("CONFLICT", "You are already on this plan");

            var currentPlan = await _planRepository.GetByIdAsync(sub.PlanId, cancellationToken);
            var newPlan = await _planRepository.GetByIdAsync(request.NewPlanId, cancellationToken);

            if (newPlan == null || !newPlan.IsActive)
                return Result<SwitchPlanResult>.FailureResult("NOT_FOUND", "Target plan not found or inactive");

            var now = DateTime.UtcNow;
            var daysRemaining = Math.Max(0, (sub.EndDate - now).TotalDays);
            var daysInCycle = Math.Max(1, (sub.EndDate - sub.StartDate).TotalDays);
            var remainingFraction = daysRemaining / daysInCycle;

            var oldPrice = currentPlan?.MonthlyPrice ?? 0m;
            var newPrice = newPlan.MonthlyPrice;
            var proratedDiff = Math.Round((newPrice - oldPrice) * (decimal)remainingFraction, 2);

            // ── Downgrade: credit as bonus points, switch immediately ──
            if (newPrice < oldPrice)
            {
                var creditAmount = Math.Abs(proratedDiff);
                var bonusPoints = (int)Math.Round(creditAmount); // 1 pt = ₹1

                // Expire old subscription
                sub.Status = SubscriptionStatus.Cancelled;
                sub.UpdatedAt = now;
                await _subRepository.UpdateAsync(sub, cancellationToken);

                // Create new subscription
                var newSub = new StudentSubscription
                {
                    Id = Guid.NewGuid(),
                    StudentId = userId.Value,
                    PlanId = request.NewPlanId,
                    StartDate = now,
                    EndDate = sub.EndDate, // preserve remaining cycle end
                    HoursUsed = 0,
                    HoursRemaining = newPlan.HoursIncluded,
                    SessionsUsed = 0,
                    Status = SubscriptionStatus.Active,
                    AutoRenew = sub.AutoRenew,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _subRepository.AddAsync(newSub, cancellationToken);

                if (bonusPoints > 0)
                {
                    await _bonusRepository.AddAsync(new BonusPoint
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId.Value,
                        Points = bonusPoints,
                        Reason = BonusPointReason.Registration, // repurposed as credit
                        CreatedAt = now,
                        UpdatedAt = now
                    }, cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result<SwitchPlanResult>.SuccessResult(new SwitchPlanResult
                {
                    ImmediateSwitch = true,
                    RequiresPayment = false,
                    NewSubscriptionId = newSub.Id,
                    CreditAmount = creditAmount,
                    Message = $"Switched to {newPlan.Name}. ₹{creditAmount:N0} credited as {bonusPoints} bonus points."
                });
            }

            // ── Upgrade: charge prorated difference via Razorpay ──
            if (proratedDiff <= 0)
                proratedDiff = 1; // minimum charge

            var (orderId, key) = await _paymentService.CreateOrderAsync(
                proratedDiff, "INR",
                new Dictionary<string, string>
                {
                    { "newPlanId", newPlan.Id.ToString() },
                    { "studentId", userId.Value.ToString() },
                    { "purpose", "plan_upgrade" }
                });

            // Create new subscription record (pending payment)
            var upgradeSub = new StudentSubscription
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                PlanId = request.NewPlanId,
                StartDate = now,
                EndDate = sub.EndDate,
                HoursUsed = 0,
                HoursRemaining = newPlan.HoursIncluded,
                SessionsUsed = 0,
                Status = SubscriptionStatus.Active, // will be confirmed on payment
                GatewayOrderId = orderId,
                AutoRenew = sub.AutoRenew,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _subRepository.AddAsync(upgradeSub, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<SwitchPlanResult>.SuccessResult(new SwitchPlanResult
            {
                ImmediateSwitch = false,
                RequiresPayment = true,
                NewSubscriptionId = upgradeSub.Id,
                GatewayOrderId = orderId,
                ChargeAmount = proratedDiff,
                RazorpayKey = key,
                Message = $"Upgrade to {newPlan.Name} requires a prorated payment of ₹{proratedDiff:N2}."
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error switching subscription plan");
            return Result<SwitchPlanResult>.FailureResult("ERROR", ex.Message);
        }
    }
}

// ─── Feature 25: Activate Plan Switch (after Razorpay upgrade payment) ───────

public class ActivatePlanSwitchCommandHandler : IRequestHandler<ActivatePlanSwitchCommand, Result<StudentSubscriptionDto>>
{
    private readonly IRepository<StudentSubscription> _subRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;

    public ActivatePlanSwitchCommandHandler(
        IRepository<StudentSubscription> subRepository,
        IRepository<SubscriptionPlan> planRepository,
        IRepository<Payment> paymentRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork)
    {
        _subRepository = subRepository;
        _planRepository = planRepository;
        _paymentRepository = paymentRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentSubscriptionDto>> Handle(ActivatePlanSwitchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentSubscriptionDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var newSub = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.PlanId == request.NewPlanId && s.GatewayOrderId == request.OrderId,
                cancellationToken);

            if (newSub == null)
                return Result<StudentSubscriptionDto>.FailureResult("NOT_FOUND", "Upgrade subscription record not found");

            var isValid = await _paymentService.VerifyPaymentSignatureAsync(request.OrderId, request.PaymentId, request.Signature);
            if (!isValid)
                return Result<StudentSubscriptionDto>.FailureResult("VALIDATION_ERROR", "Payment signature verification failed");

            // Expire the old active subscription (different plan)
            var oldSub = await _subRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active && s.Id != newSub.Id,
                cancellationToken);

            if (oldSub != null)
            {
                oldSub.Status = SubscriptionStatus.Cancelled;
                oldSub.UpdatedAt = DateTime.UtcNow;
                await _subRepository.UpdateAsync(oldSub, cancellationToken);
            }

            var plan = await _planRepository.GetByIdAsync(newSub.PlanId, cancellationToken);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                TutorId = Guid.Empty,
                BaseAmount = plan?.MonthlyPrice ?? 0,
                PlatformFee = 0,
                TotalAmount = plan?.MonthlyPrice ?? 0,
                Status = PaymentStatus.Success,
                PaymentGateway = "Razorpay",
                GatewayOrderId = request.OrderId,
                GatewayPaymentId = request.PaymentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment, cancellationToken);

            newSub.PaymentId = payment.Id;
            newSub.Status = SubscriptionStatus.Active;
            newSub.UpdatedAt = DateTime.UtcNow;

            await _subRepository.UpdateAsync(newSub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<StudentSubscriptionDto>.SuccessResult(SubHelper.ToDto(newSub, plan!));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
