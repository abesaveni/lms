using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class Session : BaseEntity
{
    public Guid TutorId { get; set; }
    public SessionType SessionType { get; set; }
    public SessionPricingType PricingType { get; set; } = SessionPricingType.Fixed;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int Duration { get; set; } // in minutes
    public decimal BasePrice { get; set; }
    public int MaxStudents { get; set; }
    public int CurrentStudents { get; set; }
    public SessionStatus Status { get; set; }
    /// <summary>
    /// DEPRECATED: Use SessionMeetLink entity instead
    /// Kept for backward compatibility
    /// </summary>
    [Obsolete("Use SessionMeetLink entity instead")]
    public string? MeetingLink { get; set; }
    public string? RecordingUrl { get; set; }
    public bool IsRecorded { get; set; }
    
    /// <summary>
    /// Google Calendar Event ID for this session
    /// </summary>
    public string? GoogleCalendarEventId { get; set; }
    
    /// <summary>
    /// Legacy field - use GoogleCalendarEventId
    /// </summary>
    [Obsolete("Use GoogleCalendarEventId instead")]
    public string? CalendarEventId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsReminderSent { get; set; }

    /// <summary>When true, only students with an active subscription can book this session.</summary>
    public bool RequiresSubscription { get; set; } = false;

    // ── Feature: Flash Sale ───────────────────────────────────────────────────
    /// <summary>Discounted price during flash sale window. Null = no flash sale.</summary>
    public decimal? FlashSalePrice { get; set; }
    /// <summary>When the flash sale ends. Session reverts to BasePrice after this.</summary>
    public DateTime? FlashSaleEndsAt { get; set; }

    // ── Feature: Instant Booking ─────────────────────────────────────────────
    /// <summary>When true, booking is auto-confirmed without tutor approval.</summary>
    public bool InstantBooking { get; set; } = false;

    // ── Feature: No-Show Protection ──────────────────────────────────────────
    /// <summary>When true, no-show students are auto-refunded and awarded 50 bonus points.</summary>
    public bool NoShowProtection { get; set; } = false;

    // Navigation Properties
    public User Tutor { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<SessionBooking> Bookings { get; set; } = new List<SessionBooking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public SessionMeetLink? MeetLink { get; set; }
}
