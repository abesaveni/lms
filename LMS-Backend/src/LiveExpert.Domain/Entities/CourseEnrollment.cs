using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Tracks a student's enrollment in a Course — full bundle or partial (N sessions).
/// </summary>
public class CourseEnrollment : BaseEntity
{
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }

    // ── Enrollment Type ───────────────────────────────────────────────────────
    public EnrollmentType EnrollmentType { get; set; } = EnrollmentType.Full;
    public int SessionsPurchased { get; set; }
    public int SessionsCompleted { get; set; }
    public int SessionsRemaining => Math.Max(0, SessionsPurchased - SessionsCompleted);

    // ── Payment ───────────────────────────────────────────────────────────────
    public decimal AmountPaid { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal TutorEarningAmount { get; set; }   // AmountPaid - PlatformFee
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public string? GatewaySignature { get; set; }

    // ── Status / Dates ────────────────────────────────────────────────────────
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime? EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }        // sessions expire after N months
    public string? CancellationReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public Course Course { get; set; } = null!;
    public User Student { get; set; } = null!;
}
