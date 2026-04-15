using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class TutorProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public string? Headline { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal HourlyRateGroup { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotStarted;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string? ResumeUrl { get; set; }
    public string? VideoIntroUrl { get; set; }
    
    /// <summary>
    /// Government ID document URL (for verification)
    /// </summary>
    public string? GovtIdUrl { get; set; }
    
    /// <summary>
    /// Onboarding step completed (1=Basic, 2=Profile, 3=Video, 4=Verification)
    /// </summary>
    public int OnboardingStep { get; set; } = 1;
    
    /// <summary>
    /// Whether profile is complete and ready for verification
    /// </summary>
    public bool IsProfileComplete { get; set; } = false;
    
    /// <summary>
    /// Whether tutor is visible in Find Tutors (only if verified)
    /// </summary>
    public bool IsVisible { get; set; } = false;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalSessions { get; set; }
    public decimal CompletionRate { get; set; }
    public int ResponseTime { get; set; } // in minutes
    public bool IsCalendarConnected { get; set; }
    public CalendarProvider? CalendarProvider { get; set; }
    
    // Additional Profile Information (from resume parsing)
    public string? Education { get; set; }
    public string? Certifications { get; set; }
    public string? Skills { get; set; }
    public string? Languages { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    
    public string? CalendarAccessToken { get; set; }
    public string? CalendarRefreshToken { get; set; }

    // ── Teaching Details ──────────────────────────────────────────────────────
    /// <summary>Teaching style tags (JSON array: ["Interactive","Project-based"])</summary>
    public string? TeachingStyles { get; set; }

    /// <summary>Age groups tutor teaches (JSON array: ["Kids 6-12","Teens","Adults"])</summary>
    public string? AgeGroups { get; set; }

    /// <summary>Whether this tutor offers trial sessions</summary>
    public bool TrialAvailable { get; set; } = false;

    /// <summary>Default trial duration in minutes (0 = disabled)</summary>
    public int TrialDurationMinutes { get; set; } = 30;

    /// <summary>Default trial price (0 = free)</summary>
    public decimal TrialPrice { get; set; } = 0;

    /// <summary>Whether a verification-nudge reminder has already been sent to this tutor.</summary>
    public bool VerificationReminderSent { get; set; } = false;

    /// <summary>Referral code tutors can share to refer other tutors. Reward: bonus on referred tutor's first month earnings.</summary>
    public string? TutorReferralCode { get; set; }

    // ── Background Check Badge ───────────────────────────────────────────────
    /// <summary>True when admin has verified the tutor's background check. Shown as a badge on their profile.</summary>
    public bool HasBackgroundCheck { get; set; } = false;
    public DateTime? BackgroundCheckDate { get; set; }

    // ── Auto-Payout ───────────────────────────────────────────────────────────
    public AutoPayoutSchedule AutoPayoutSchedule { get; set; } = AutoPayoutSchedule.Disabled;

    /// <summary>Minimum balance (INR) before auto-payout fires</summary>
    public decimal AutoPayoutMinimumAmount { get; set; } = 1000;

    // ── Navigation Properties ─────────────────────────────────────────────────
    public User User { get; set; } = null!;
    public TutorVerification? Verification { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<TutorSubjectRate> SubjectRates { get; set; } = new List<TutorSubjectRate>();
}
