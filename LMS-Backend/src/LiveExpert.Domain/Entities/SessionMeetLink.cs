using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Stores encrypted Google Meet links for sessions
/// Never exposed directly to frontend - only used server-side
/// </summary>
public class SessionMeetLink : BaseEntity
{
    public Guid SessionId { get; set; }
    
    /// <summary>
    /// Encrypted Google Meet URL
    /// </summary>
    public string MeetUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Google Calendar Event ID associated with this Meet
    /// </summary>
    public string? CalendarEventId { get; set; }
    
    /// <summary>
    /// Whether this link is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When the session actually started (if started)
    /// </summary>
    public DateTime? SessionStartedAt { get; set; }
    
    /// <summary>
    /// When the session ended (if ended)
    /// </summary>
    public DateTime? SessionEndedAt { get; set; }

    // Navigation Properties
    public Session Session { get; set; } = null!;
}
