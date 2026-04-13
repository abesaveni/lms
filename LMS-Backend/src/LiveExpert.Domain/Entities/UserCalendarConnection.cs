using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Stores Google Calendar OAuth tokens for ALL users (Students and Tutors)
/// Mandatory for using the platform
/// </summary>
public class UserCalendarConnection : BaseEntity
{
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Calendar provider (currently only Google)
    /// </summary>
    public CalendarProvider Provider { get; set; } = CalendarProvider.Google;
    
    /// <summary>
    /// Encrypted Google OAuth Access Token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted Google OAuth Refresh Token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Token expiry date/time
    /// </summary>
    public DateTime TokenExpiry { get; set; }
    
    /// <summary>
    /// Google user email associated with this token
    /// </summary>
    public string GoogleEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the connection is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Last time token was refreshed
    /// </summary>
    public DateTime? LastRefreshedAt { get; set; }
    
    /// <summary>
    /// When the connection was first established
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
}
