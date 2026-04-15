using LiveExpert.Application.Common;
using LiveExpert.Application.Features.SubscriptionPlans.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.SubscriptionPlans.Handlers;

// Feature 10: Subscription plans

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly ILogger<GetSubscriptionPlansQueryHandler> _logger;

    public GetSubscriptionPlansQueryHandler(
        IRepository<SubscriptionPlan> planRepository,
        ILogger<GetSubscriptionPlansQueryHandler> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var plans = await _planRepository.FindAsync(p => p.IsActive, cancellationToken);
            var dtos = plans.OrderBy(p => p.MonthlyPrice).Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                MonthlyPrice = p.MonthlyPrice,
                HoursIncluded = p.HoursIncluded,
                IsActive = p.IsActive
            }).ToList();

            return Result<List<SubscriptionPlanDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans");
            return Result<List<SubscriptionPlanDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, Result<StudentSubscriptionDto?>>
{
    private readonly IRepository<StudentSubscription> _subscriptionRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMySubscriptionQueryHandler> _logger;

    public GetMySubscriptionQueryHandler(
        IRepository<StudentSubscription> subscriptionRepository,
        IRepository<SubscriptionPlan> planRepository,
        ICurrentUserService currentUserService,
        ILogger<GetMySubscriptionQueryHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<StudentSubscriptionDto?>> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentSubscriptionDto?>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var sub = await _subscriptionRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active,
                cancellationToken);

            if (sub == null)
                return Result<StudentSubscriptionDto?>.SuccessResult(null);

            var plan = await _planRepository.GetByIdAsync(sub.PlanId, cancellationToken);

            return Result<StudentSubscriptionDto?>.SuccessResult(new StudentSubscriptionDto
            {
                Id = sub.Id,
                StudentId = sub.StudentId,
                PlanId = sub.PlanId,
                PlanName = plan?.Name ?? "",
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                HoursUsed = sub.HoursUsed,
                HoursRemaining = sub.HoursRemaining,
                Status = sub.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription");
            return Result<StudentSubscriptionDto?>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, Result<SubscriptionOrderDto>>
{
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IRepository<StudentSubscription> _subscriptionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscribeCommandHandler> _logger;

    public SubscribeCommandHandler(
        IRepository<SubscriptionPlan> planRepository,
        IRepository<StudentSubscription> subscriptionRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<SubscribeCommandHandler> logger)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
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

            var existing = await _subscriptionRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active, cancellationToken);
            if (existing != null)
                return Result<SubscriptionOrderDto>.FailureResult("CONFLICT", "You already have an active subscription");

            var (orderId, key) = await _paymentService.CreateOrderAsync(
                plan.MonthlyPrice,
                "INR",
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
                Status = SubscriptionStatus.Active,
                GatewayOrderId = orderId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _subscriptionRepository.AddAsync(sub, cancellationToken);
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

public class ActivateSubscriptionPlanCommandHandler : IRequestHandler<ActivateSubscriptionPlanCommand, Result<StudentSubscriptionDto>>
{
    private readonly IRepository<StudentSubscription> _subscriptionRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateSubscriptionPlanCommandHandler> _logger;

    public ActivateSubscriptionPlanCommandHandler(
        IRepository<StudentSubscription> subscriptionRepository,
        IRepository<SubscriptionPlan> planRepository,
        IRepository<Payment> paymentRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<ActivateSubscriptionPlanCommandHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
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
            var sub = await _subscriptionRepository.FirstOrDefaultAsync(
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

            await _subscriptionRepository.UpdateAsync(sub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<StudentSubscriptionDto>.SuccessResult(new StudentSubscriptionDto
            {
                Id = sub.Id,
                StudentId = sub.StudentId,
                PlanId = sub.PlanId,
                PlanName = plan?.Name ?? "",
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                HoursUsed = sub.HoursUsed,
                HoursRemaining = sub.HoursRemaining,
                Status = sub.Status
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error activating subscription");
            return Result<StudentSubscriptionDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly IRepository<StudentSubscription> _subscriptionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelSubscriptionCommandHandler> _logger;

    public CancelSubscriptionCommandHandler(
        IRepository<StudentSubscription> subscriptionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CancelSubscriptionCommandHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sub = await _subscriptionRepository.FirstOrDefaultAsync(
                s => s.StudentId == userId.Value && s.Status == SubscriptionStatus.Active, cancellationToken);

            if (sub == null)
                return Result.FailureResult("NOT_FOUND", "No active subscription found");

            sub.Status = SubscriptionStatus.Cancelled;
            sub.UpdatedAt = DateTime.UtcNow;

            await _subscriptionRepository.UpdateAsync(sub, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.SuccessResult("Subscription cancelled");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error cancelling subscription");
            return Result.FailureResult("ERROR", ex.Message);
        }
    }
}
