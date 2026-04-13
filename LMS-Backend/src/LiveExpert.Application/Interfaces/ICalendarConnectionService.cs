namespace LiveExpert.Application.Interfaces;

/// <summary>
/// Service for managing Google Calendar connections for ALL users (Students and Tutors)
/// </summary>
public interface ICalendarConnectionService
{
    /// <summary>
    /// Get authorization URL for Google Calendar OAuth
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(Guid userId, string frontendRedirectUri, string backendCallbackUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exchange authorization code for tokens and store them
    /// </summary>
    Task<bool> ExchangeCodeForTokensAsync(Guid userId, string code, string redirectUri, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get valid access token for user (refresh if needed)
    /// </summary>
    Task<string?> GetValidAccessTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if user has connected Google Calendar (MANDATORY)
    /// </summary>
    Task<bool> IsCalendarConnectedAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoke and remove Google tokens for user
    /// </summary>
    Task<bool> RevokeConnectionAsync(Guid userId, CancellationToken cancellationToken = default);
}
