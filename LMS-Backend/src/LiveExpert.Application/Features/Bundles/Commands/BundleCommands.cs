using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Bundles.Commands;

// Feature 9: Session bundles

public class CreateBundleCommand : IRequest<Result<SessionBundleDto>>
{
    public Guid? SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SessionCount { get; set; }
    public decimal TotalPrice { get; set; }
    public int ValidityDays { get; set; }
    public decimal DiscountPercentage { get; set; }
}

public class PurchaseBundleCommand : IRequest<Result<BundlePurchaseOrderDto>>
{
    public Guid BundleId { get; set; }
}

public class ActivateBundlePurchaseCommand : IRequest<Result<BundlePurchaseDto>>
{
    public Guid BundleId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class GetTutorBundlesQuery : IRequest<Result<List<SessionBundleDto>>>
{
    public Guid TutorId { get; set; }
}

public class GetMyBundlesQuery : IRequest<Result<List<BundlePurchaseDto>>>
{
}

public class SessionBundleDto
{
    public Guid Id { get; set; }
    public Guid TutorId { get; set; }
    public Guid? SubjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SessionCount { get; set; }
    public decimal TotalPrice { get; set; }
    public int ValidityDays { get; set; }
    public bool IsActive { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BundlePurchaseOrderDto
{
    public Guid PurchaseId { get; set; }
    public string GatewayOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string RazorpayKey { get; set; } = string.Empty;
}

public class BundlePurchaseDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid BundleId { get; set; }
    public string BundleTitle { get; set; } = string.Empty;
    public int SessionsRemaining { get; set; }
    public DateTime ExpiresAt { get; set; }
    public BundleStatus Status { get; set; }
}
