using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Tutor payout requests for withdrawing earnings
/// </summary>
public class PayoutRequest : BaseEntity
{
    public Guid TutorId { get; set; }
    public Guid BankAccountId { get; set; }
    
    /// <summary>
    /// Requested payout amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Status of payout request
    /// </summary>
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    
    /// <summary>
    /// When payout was requested
    /// </summary>
    public DateTime RequestedAt { get; set; }
    
    /// <summary>
    /// When payout was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Admin who processed the request
    /// </summary>
    public Guid? ProcessedBy { get; set; }
    
    /// <summary>
    /// Admin notes or rejection reason
    /// </summary>
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// Transaction reference (if paid via Razorpay)
    /// </summary>
    public string? TransactionReference { get; set; }
    
    /// <summary>
    /// Payment method used
    /// </summary>
    public string PaymentMethod { get; set; } = "Bank Transfer";

    // Navigation Properties
    public User Tutor { get; set; } = null!;
    public BankAccount BankAccount { get; set; } = null!;
    public User? ProcessedByUser { get; set; }
    public ICollection<TutorEarning> Earnings { get; set; } = new List<TutorEarning>();
}
