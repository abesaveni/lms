using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Stores encrypted Google OAuth tokens for tutors to access Google Calendar API
/// </summary>
public class TutorGoogleTokens : BaseEntity
{
    public Guid TutorId { get; set; }
    
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
    /// Whether the token is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Last time token was refreshed
    /// </summary>
    public DateTime? LastRefreshedAt { get; set; }

    // Navigation Properties
    public User Tutor { get; set; } = null!;
}
