using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Tracks tutor earnings from sessions
/// </summary>
public class TutorEarning : BaseEntity
{
    public Guid TutorId { get; set; }
    
    /// <summary>
    /// Source of earnings (Session)
    /// </summary>
    public string SourceType { get; set; } = string.Empty; // "Session"
    
    /// <summary>
    /// ID of the session or course
    /// </summary>
    public Guid SourceId { get; set; }
    
    /// <summary>
    /// Earnings amount (in currency)
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Status of earnings
    /// </summary>
    public EarningStatus Status { get; set; } = EarningStatus.Pending;
    
    /// <summary>
    /// When earnings become available for withdrawal
    /// </summary>
    public DateTime? AvailableAt { get; set; }
    
    /// <summary>
    /// When earnings were paid out
    /// </summary>
    public DateTime? PaidAt { get; set; }
    
    /// <summary>
    /// Related payout request ID (if paid out)
    /// </summary>
    public Guid? PayoutRequestId { get; set; }
    
    /// <summary>
    /// Platform commission percentage
    /// </summary>
    public decimal CommissionPercentage { get; set; }
    
    /// <summary>
    /// Platform commission amount
    /// </summary>
    public decimal CommissionAmount { get; set; }
    
    /// <summary>
    /// Net amount after commission
    /// </summary>
    public decimal NetAmount => Amount - CommissionAmount;

    // Navigation Properties
    public User Tutor { get; set; } = null!;
    public PayoutRequest? PayoutRequest { get; set; }
}
