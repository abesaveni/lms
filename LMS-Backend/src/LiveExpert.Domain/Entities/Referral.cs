using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Tracks referral relationships and bonus points
/// </summary>
public class Referral : BaseEntity
{
    public Guid ReferrerUserId { get; set; }
    public Guid ReferredUserId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public ReferralStatus Status { get; set; }
    
    /// <summary>
    /// Bonus points to be awarded to referrer
    /// </summary>
    public decimal BonusCredits { get; set; }
    
    /// <summary>
    /// When the bonus was actually rewarded (null if pending)
    /// </summary>
    public DateTime? RewardedAt { get; set; }
    
    /// <summary>
    /// What triggered the bonus release (SessionId)
    /// </summary>
    public Guid? TriggerReferenceId { get; set; }
    
    /// <summary>
    /// Type of activity that triggered the bonus
    /// </summary>
    public string? TriggerActivityType { get; set; } // "Session"

    // Navigation Properties
    public User Referrer { get; set; } = null!;
    public User ReferredUser { get; set; } = null!;
}
