using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

/// <summary>Coupon/promo code that gives a percentage or flat discount on session booking.</summary>
public class CouponCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;           // e.g. "WELCOME20"
    public string? Description { get; set; }
    public CouponDiscountType DiscountType { get; set; }       // Percentage | Flat
    public decimal DiscountValue { get; set; }                 // e.g. 20 (%) or 200 (₹)
    public decimal? MaxDiscountAmount { get; set; }            // Cap for percentage coupons
    public decimal? MinOrderAmount { get; set; }               // Min booking value to apply
    public int? MaxUses { get; set; }                          // null = unlimited
    public int UsedCount { get; set; } = 0;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? TutorId { get; set; }                         // null = platform-wide; set = tutor-specific
    public Guid? CreatedByAdminId { get; set; }

    // Navigation
    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Guid StudentId { get; set; }
    public Guid BookingId { get; set; }
    public decimal DiscountApplied { get; set; }

    // Navigation
    public CouponCode Coupon { get; set; } = null!;
    public User Student { get; set; } = null!;
    public SessionBooking Booking { get; set; } = null!;
}

public enum CouponDiscountType { Percentage, Flat }
