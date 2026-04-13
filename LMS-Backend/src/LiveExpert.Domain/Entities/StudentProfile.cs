using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class StudentProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? LearningGoals { get; set; }
    public string? PreferredSubjects { get; set; }
    public bool IsCalendarConnected { get; set; }
    public CalendarProvider? CalendarProvider { get; set; }
    public string? CalendarAccessToken { get; set; }
    public string? CalendarRefreshToken { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public Guid? ReferredBy { get; set; }

    // AI Resume
    public string? ResumeData { get; set; }
    public string? ResumeType { get; set; }
    public DateTime? ResumeLastUpdatedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public User? ReferredByUser { get; set; }
}
