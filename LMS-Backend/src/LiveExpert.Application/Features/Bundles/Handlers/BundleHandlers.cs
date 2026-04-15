using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Bundles.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.Bundles.Handlers;

// Feature 9: Session bundles

public class CreateBundleCommandHandler : IRequestHandler<CreateBundleCommand, Result<SessionBundleDto>>
{
    private readonly IRepository<SessionBundle> _bundleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBundleCommandHandler> _logger;

    public CreateBundleCommandHandler(
        IRepository<SessionBundle> bundleRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CreateBundleCommandHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SessionBundleDto>> Handle(CreateBundleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<SessionBundleDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.SessionCount <= 0)
                return Result<SessionBundleDto>.FailureResult("VALIDATION_ERROR", "SessionCount must be greater than 0");
            if (request.TotalPrice < 0)
                return Result<SessionBundleDto>.FailureResult("VALIDATION_ERROR", "TotalPrice cannot be negative");
            if (request.ValidityDays <= 0)
                return Result<SessionBundleDto>.FailureResult("VALIDATION_ERROR", "ValidityDays must be greater than 0");

            var bundle = new SessionBundle
            {
                Id = Guid.NewGuid(),
                TutorId = userId.Value,
                SubjectId = request.SubjectId,
                Title = request.Title,
                Description = request.Description,
                SessionCount = request.SessionCount,
                TotalPrice = request.TotalPrice,
                ValidityDays = request.ValidityDays,
                IsActive = true,
                DiscountPercentage = request.DiscountPercentage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _bundleRepository.AddAsync(bundle, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<SessionBundleDto>.SuccessResult(MapToDto(bundle));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating bundle");
            return Result<SessionBundleDto>.FailureResult("ERROR", ex.Message);
        }
    }

    private static SessionBundleDto MapToDto(SessionBundle b) => new()
    {
        Id = b.Id,
        TutorId = b.TutorId,
        SubjectId = b.SubjectId,
        Title = b.Title,
        Description = b.Description,
        SessionCount = b.SessionCount,
        TotalPrice = b.TotalPrice,
        ValidityDays = b.ValidityDays,
        IsActive = b.IsActive,
        DiscountPercentage = b.DiscountPercentage,
        CreatedAt = b.CreatedAt
    };
}

public class PurchaseBundleCommandHandler : IRequestHandler<PurchaseBundleCommand, Result<BundlePurchaseOrderDto>>
{
    private readonly IRepository<SessionBundle> _bundleRepository;
    private readonly IRepository<BundlePurchase> _purchaseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PurchaseBundleCommandHandler> _logger;

    public PurchaseBundleCommandHandler(
        IRepository<SessionBundle> bundleRepository,
        IRepository<BundlePurchase> purchaseRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<PurchaseBundleCommandHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _purchaseRepository = purchaseRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BundlePurchaseOrderDto>> Handle(PurchaseBundleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<BundlePurchaseOrderDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bundle = await _bundleRepository.GetByIdAsync(request.BundleId, cancellationToken);
            if (bundle == null || !bundle.IsActive)
                return Result<BundlePurchaseOrderDto>.FailureResult("NOT_FOUND", "Bundle not found or inactive");

            var (orderId, key) = await _paymentService.CreateOrderAsync(
                bundle.TotalPrice,
                "INR",
                new Dictionary<string, string>
                {
                    { "bundleId", bundle.Id.ToString() },
                    { "studentId", userId.Value.ToString() },
                    { "purpose", "bundle_purchase" }
                });

            var purchase = new BundlePurchase
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                BundleId = request.BundleId,
                SessionsRemaining = bundle.SessionCount,
                ExpiresAt = DateTime.UtcNow.AddDays(bundle.ValidityDays),
                Status = BundleStatus.Active,
                GatewayOrderId = orderId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _purchaseRepository.AddAsync(purchase, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<BundlePurchaseOrderDto>.SuccessResult(new BundlePurchaseOrderDto
            {
                PurchaseId = purchase.Id,
                GatewayOrderId = orderId,
                Amount = bundle.TotalPrice,
                Currency = "INR",
                RazorpayKey = key
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error initiating bundle purchase");
            return Result<BundlePurchaseOrderDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class ActivateBundlePurchaseCommandHandler : IRequestHandler<ActivateBundlePurchaseCommand, Result<BundlePurchaseDto>>
{
    private readonly IRepository<BundlePurchase> _purchaseRepository;
    private readonly IRepository<SessionBundle> _bundleRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateBundlePurchaseCommandHandler> _logger;

    public ActivateBundlePurchaseCommandHandler(
        IRepository<BundlePurchase> purchaseRepository,
        IRepository<SessionBundle> bundleRepository,
        IRepository<Payment> paymentRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<ActivateBundlePurchaseCommandHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _bundleRepository = bundleRepository;
        _paymentRepository = paymentRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BundlePurchaseDto>> Handle(ActivateBundlePurchaseCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<BundlePurchaseDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var purchase = await _purchaseRepository.FirstOrDefaultAsync(
                p => p.BundleId == request.BundleId && p.StudentId == userId.Value && p.GatewayOrderId == request.OrderId,
                cancellationToken);

            if (purchase == null)
                return Result<BundlePurchaseDto>.FailureResult("NOT_FOUND", "Purchase record not found");

            var isValid = await _paymentService.VerifyPaymentSignatureAsync(request.OrderId, request.PaymentId, request.Signature);
            if (!isValid)
                return Result<BundlePurchaseDto>.FailureResult("VALIDATION_ERROR", "Payment signature verification failed");

            var bundle = await _bundleRepository.GetByIdAsync(purchase.BundleId, cancellationToken);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                TutorId = bundle?.TutorId ?? Guid.Empty,
                BaseAmount = bundle?.TotalPrice ?? 0,
                PlatformFee = 0,
                TotalAmount = bundle?.TotalPrice ?? 0,
                Status = PaymentStatus.Success,
                PaymentGateway = "Razorpay",
                GatewayOrderId = request.OrderId,
                GatewayPaymentId = request.PaymentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment, cancellationToken);

            purchase.PaymentId = payment.Id;
            purchase.Status = BundleStatus.Active;
            purchase.UpdatedAt = DateTime.UtcNow;

            await _purchaseRepository.UpdateAsync(purchase, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<BundlePurchaseDto>.SuccessResult(new BundlePurchaseDto
            {
                Id = purchase.Id,
                StudentId = purchase.StudentId,
                BundleId = purchase.BundleId,
                BundleTitle = bundle?.Title ?? "",
                SessionsRemaining = purchase.SessionsRemaining,
                ExpiresAt = purchase.ExpiresAt,
                Status = purchase.Status
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error activating bundle purchase");
            return Result<BundlePurchaseDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetTutorBundlesQueryHandler : IRequestHandler<GetTutorBundlesQuery, Result<List<SessionBundleDto>>>
{
    private readonly IRepository<SessionBundle> _bundleRepository;
    private readonly ILogger<GetTutorBundlesQueryHandler> _logger;

    public GetTutorBundlesQueryHandler(
        IRepository<SessionBundle> bundleRepository,
        ILogger<GetTutorBundlesQueryHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _logger = logger;
    }

    public async Task<Result<List<SessionBundleDto>>> Handle(GetTutorBundlesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bundles = await _bundleRepository.FindAsync(b => b.TutorId == request.TutorId && b.IsActive, cancellationToken);
            var dtos = bundles.OrderByDescending(b => b.CreatedAt).Select(b => new SessionBundleDto
            {
                Id = b.Id,
                TutorId = b.TutorId,
                SubjectId = b.SubjectId,
                Title = b.Title,
                Description = b.Description,
                SessionCount = b.SessionCount,
                TotalPrice = b.TotalPrice,
                ValidityDays = b.ValidityDays,
                IsActive = b.IsActive,
                DiscountPercentage = b.DiscountPercentage,
                CreatedAt = b.CreatedAt
            }).ToList();

            return Result<List<SessionBundleDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tutor bundles");
            return Result<List<SessionBundleDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetMyBundlesQueryHandler : IRequestHandler<GetMyBundlesQuery, Result<List<BundlePurchaseDto>>>
{
    private readonly IRepository<BundlePurchase> _purchaseRepository;
    private readonly IRepository<SessionBundle> _bundleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyBundlesQueryHandler> _logger;

    public GetMyBundlesQueryHandler(
        IRepository<BundlePurchase> purchaseRepository,
        IRepository<SessionBundle> bundleRepository,
        ICurrentUserService currentUserService,
        ILogger<GetMyBundlesQueryHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _bundleRepository = bundleRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<BundlePurchaseDto>>> Handle(GetMyBundlesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<BundlePurchaseDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var purchases = (await _purchaseRepository.FindAsync(p => p.StudentId == userId.Value, cancellationToken)).ToList();
            var bundleIds = purchases.Select(p => p.BundleId).Distinct().ToList();
            var bundles = (await _bundleRepository.FindAsync(b => bundleIds.Contains(b.Id), cancellationToken))
                .ToDictionary(b => b.Id);

            var dtos = purchases.OrderByDescending(p => p.CreatedAt).Select(p =>
            {
                bundles.TryGetValue(p.BundleId, out var bundle);
                return new BundlePurchaseDto
                {
                    Id = p.Id,
                    StudentId = p.StudentId,
                    BundleId = p.BundleId,
                    BundleTitle = bundle?.Title ?? "",
                    SessionsRemaining = p.SessionsRemaining,
                    ExpiresAt = p.ExpiresAt,
                    Status = p.Status
                };
            }).ToList();

            return Result<List<BundlePurchaseDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my bundles");
            return Result<List<BundlePurchaseDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}
