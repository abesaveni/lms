using LiveExpert.Application.Common;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Coupons;

// ── Admin: Create Coupon ──────────────────────────────────────────────────────
public class CreateCouponCommand : IRequest<Result<CouponDto>>
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MaxUses { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? TutorId { get; set; }
}

// ── Admin: Toggle Coupon Active ────────────────────────────────────────────────
public class ToggleCouponCommand : IRequest<Result>
{
    public Guid CouponId { get; set; }
    public bool IsActive { get; set; }
}

// ── Admin: List All Coupons ────────────────────────────────────────────────────
public class ListCouponsQuery : IRequest<Result<List<CouponDto>>> { }

// ── Student: Validate + Apply Coupon ──────────────────────────────────────────
public class ValidateCouponQuery : IRequest<Result<CouponValidationResult>>
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public Guid? TutorId { get; set; }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────
public class CouponDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public Guid CouponId { get; set; }
}
