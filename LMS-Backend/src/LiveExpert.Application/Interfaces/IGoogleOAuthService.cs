namespace LiveExpert.Application.Interfaces;

/// <summary>
/// Service for managing Google OAuth tokens for tutors
/// </summary>
public interface IGoogleOAuthService
{
    /// <summary>
    /// Get authorization URL for Google Calendar OAuth
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(Guid tutorId, string redirectUri, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exchange authorization code for tokens and store them
    /// </summary>
    Task<bool> ExchangeCodeForTokensAsync(Guid tutorId, string code, string redirectUri, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get valid access token for tutor (refresh if needed)
    /// </summary>
    Task<string?> GetValidAccessTokenAsync(Guid tutorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if tutor has connected Google Calendar
    /// </summary>
    Task<bool> IsGoogleCalendarConnectedAsync(Guid tutorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoke and remove Google tokens for tutor
    /// </summary>
    Task<bool> RevokeTokensAsync(Guid tutorId, CancellationToken cancellationToken = default);
}
