using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TutorId { get; set; }
    public Guid SessionId { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public PaymentStatus Status { get; set; }
    public string PaymentGateway { get; set; } = "Razorpay";
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public string? GatewaySignature { get; set; }
    public string? PaymentMethod { get; set; }
    public string? FailureReason { get; set; }
    public string? Metadata { get; set; } // JSON
    public DateTime? ProcessedAt { get; set; }

    // Navigation Properties
    public User Student { get; set; } = null!;
    public User Tutor { get; set; } = null!;
    public Session Session { get; set; } = null!;
}

public class WithdrawalRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public WithdrawalStatus Status { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? TransactionId { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public BankAccount? BankAccount { get; set; }
    public User? ProcessedByUser { get; set; }
}

public class BankAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty; // Encrypted
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public AccountType AccountType { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsPrimary { get; set; }
    
    // Alias for IsPrimary (for backward compatibility)
    public bool IsDefault
    {
        get => IsPrimary;
        set => IsPrimary = value;
    }

    // Navigation Properties
    public User User { get; set; } = null!;
}
