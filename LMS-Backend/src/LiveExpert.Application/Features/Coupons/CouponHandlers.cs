using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Coupons;

// ── Create Coupon ────────────────────────────────────────────────────────────
public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Result<CouponDto>>
{
    private readonly IRepository<CouponCode> _coupons;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public CreateCouponCommandHandler(IRepository<CouponCode> coupons, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _coupons = coupons; _currentUser = currentUser; _uow = uow;
    }

    public async Task<Result<CouponDto>> Handle(CreateCouponCommand cmd, CancellationToken ct)
    {
        // Ensure code is unique (case-insensitive)
        var upper = cmd.Code.Trim().ToUpperInvariant();
        var existing = await _coupons.FirstOrDefaultAsync(c => c.Code == upper);
        if (existing != null) return Result<CouponDto>.FailureResult("COUPON_EXISTS", "A coupon with this code already exists.");

        var coupon = new CouponCode
        {
            Code = upper,
            Description = cmd.Description,
            DiscountType = cmd.DiscountType,
            DiscountValue = cmd.DiscountValue,
            MaxDiscountAmount = cmd.MaxDiscountAmount,
            MinOrderAmount = cmd.MinOrderAmount,
            MaxUses = cmd.MaxUses,
            ExpiresAt = cmd.ExpiresAt,
            TutorId = cmd.TutorId,
            IsActive = true,
            CreatedByAdminId = _currentUser.UserId,
        };

        await _coupons.AddAsync(coupon);
        await _uow.SaveChangesAsync(ct);

        return Result<CouponDto>.SuccessResult(ToDto(coupon));
    }

    private static CouponDto ToDto(CouponCode c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Description = c.Description,
        DiscountType = c.DiscountType.ToString(),
        DiscountValue = c.DiscountValue,
        MaxDiscountAmount = c.MaxDiscountAmount,
        MinOrderAmount = c.MinOrderAmount,
        MaxUses = c.MaxUses,
        UsedCount = c.UsedCount,
        ExpiresAt = c.ExpiresAt,
        IsActive = c.IsActive,
    };
}

// ── Toggle Coupon ────────────────────────────────────────────────────────────
public class ToggleCouponCommandHandler : IRequestHandler<ToggleCouponCommand, Result>
{
    private readonly IRepository<CouponCode> _coupons;
    private readonly IUnitOfWork _uow;

    public ToggleCouponCommandHandler(IRepository<CouponCode> coupons, IUnitOfWork uow)
    {
        _coupons = coupons; _uow = uow;
    }

    public async Task<Result> Handle(ToggleCouponCommand cmd, CancellationToken ct)
    {
        var coupon = await _coupons.FirstOrDefaultAsync(c => c.Id == cmd.CouponId);
        if (coupon == null) return Result.FailureResult("NOT_FOUND", "Coupon not found.");

        coupon.IsActive = cmd.IsActive;
        await _coupons.UpdateAsync(coupon);
        await _uow.SaveChangesAsync(ct);
        return Result.SuccessResult();
    }
}

// ── Validate Coupon ──────────────────────────────────────────────────────────
public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, Result<CouponValidationResult>>
{
    private readonly IRepository<CouponCode> _coupons;
    private readonly IRepository<CouponUsage> _usages;
    private readonly ICurrentUserService _currentUser;

    public ValidateCouponQueryHandler(IRepository<CouponCode> coupons, IRepository<CouponUsage> usages, ICurrentUserService currentUser)
    {
        _coupons = coupons; _usages = usages; _currentUser = currentUser;
    }

    public async Task<Result<CouponValidationResult>> Handle(ValidateCouponQuery query, CancellationToken ct)
    {
        var upper = query.Code.Trim().ToUpperInvariant();
        var coupon = await _coupons.FirstOrDefaultAsync(c => c.Code == upper && c.IsActive);
        if (coupon == null)
            return Result<CouponValidationResult>.SuccessResult(new() { IsValid = false, Message = "Invalid or expired coupon code." });

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
            return Result<CouponValidationResult>.SuccessResult(new() { IsValid = false, Message = "This coupon has expired." });

        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
            return Result<CouponValidationResult>.SuccessResult(new() { IsValid = false, Message = "This coupon has reached its usage limit." });

        if (coupon.TutorId.HasValue && coupon.TutorId != query.TutorId)
            return Result<CouponValidationResult>.SuccessResult(new() { IsValid = false, Message = "This coupon is not valid for this tutor." });

        if (coupon.MinOrderAmount.HasValue && query.OrderAmount < coupon.MinOrderAmount.Value)
            return Result<CouponValidationResult>.SuccessResult(new() { IsValid = false, Message = $"Minimum order amount is ₹{coupon.MinOrderAmount.Value} to use this coupon." });

        // Check if this student already used this coupon
        var studentId = _currentUser.UserId;
        if (studentId.HasValue)
        {
            var alreadyUsed = await _usages.FirstOrDefaultAsync(u => u.CouponId == coupon.Id && u.StudentId == studentId.Value);
            if (alreadyUsed != null)
                return Result<CouponValidationResult>.SuccessResult(new() { IsValid = false, Message = "You have already used this coupon." });
        }

        decimal discount = coupon.DiscountType == CouponDiscountType.Percentage
            ? Math.Min(query.OrderAmount * coupon.DiscountValue / 100m, coupon.MaxDiscountAmount ?? decimal.MaxValue)
            : Math.Min(coupon.DiscountValue, query.OrderAmount);

        return Result<CouponValidationResult>.SuccessResult(new()
        {
            IsValid = true,
            Message = $"Coupon applied! You save ₹{discount:0.##}",
            DiscountAmount = discount,
            FinalAmount = query.OrderAmount - discount,
            CouponId = coupon.Id,
        });
    }
}
