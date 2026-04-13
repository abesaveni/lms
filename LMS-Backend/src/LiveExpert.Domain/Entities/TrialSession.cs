using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// A trial / intro session offered by a tutor to a prospective student.
/// One trial allowed per (student, tutor) pair.
/// </summary>
public class TrialSession : BaseEntity
{
    public Guid TutorId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? CourseId { get; set; }             // optional — which course the trial is for

    // ── Scheduling ────────────────────────────────────────────────────────────
    public DateTime? ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string? MeetingLink { get; set; }

    // ── Pricing ───────────────────────────────────────────────────────────────
    public decimal Price { get; set; } = 0;         // 0 = free
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }

    // ── Status ────────────────────────────────────────────────────────────────
    public TrialSessionStatus Status { get; set; } = TrialSessionStatus.Pending;
    public DateTime? CompletedAt { get; set; }

    // ── Feedback & Conversion ─────────────────────────────────────────────────
    public string? StudentFeedback { get; set; }
    public int? StudentRating { get; set; }         // 1–5
    public bool ConvertedToEnrollment { get; set; }
    public Guid? EnrollmentId { get; set; }         // set when student enrolls after trial

    // ── Navigation ────────────────────────────────────────────────────────────
    public User Tutor { get; set; } = null!;
    public User Student { get; set; } = null!;
    public Course? Course { get; set; }
}
