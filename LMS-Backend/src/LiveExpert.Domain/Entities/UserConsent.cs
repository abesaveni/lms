using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Tracks Google OAuth consents separately for Login and Calendar
/// </summary>
public class UserConsent : AuditableEntity
{
    public Guid UserId { get; set; }
    public ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }
    public DateTime? GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}
