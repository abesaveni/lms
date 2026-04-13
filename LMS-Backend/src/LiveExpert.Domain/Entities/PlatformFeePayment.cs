namespace LiveExpert.Domain.Entities;

/// <summary>
/// Records each ₹99/month platform subscription payment — used for admin revenue reporting.
/// </summary>
public class PlatformFeePayment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public decimal Amount { get; set; }
    public string GatewayOrderId { get; set; } = string.Empty;
    public string GatewayPaymentId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Student { get; set; } = null!;
}
