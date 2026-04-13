using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Stores cookie consent preferences for users (both anonymous and authenticated)
/// </summary>
public class CookieConsent : AuditableEntity
{
    public Guid? UserId { get; set; }
    public bool Necessary { get; set; } = true; // Always true, cannot be disabled
    public bool Functional { get; set; }
    public bool Analytics { get; set; }
    public bool Marketing { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ConsentGivenAt { get; set; }
    public DateTime? ConsentUpdatedAt { get; set; }

    // Navigation Property
    public User? User { get; set; }
}
