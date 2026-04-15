using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class SessionBooking : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public int? HoursBooked { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal PointsDiscount { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool AttendanceMarked { get; set; }
    public bool Attended { get; set; }
    public DateTime? AttendedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? CancellationReason { get; set; }

    // Feature 1: Pre-session questionnaire
    public string? Goals { get; set; }
    public string? CurrentLevel { get; set; }
    public string? Topics { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundProcessedAt { get; set; }
    public Guid? PaymentId { get; set; }

    // Navigation Properties
    public Session Session { get; set; } = null!;
    public User Student { get; set; } = null!;
    public Payment? Payment { get; set; }
}
