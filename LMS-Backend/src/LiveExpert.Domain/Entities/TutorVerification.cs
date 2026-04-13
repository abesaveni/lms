using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Tracks tutor verification status and admin review
/// </summary>
public class TutorVerification : BaseEntity
{
    public Guid TutorId { get; set; }
    
    /// <summary>
    /// Verification status
    /// </summary>
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    
    /// <summary>
    /// Admin notes/reason for approval/rejection
    /// </summary>
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// Admin who verified/rejected
    /// </summary>
    public Guid? VerifiedBy { get; set; }
    
    /// <summary>
    /// When verification was completed
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
    
    /// <summary>
    /// Rejection reason (if rejected)
    /// </summary>
    public string? RejectionReason { get; set; }
    
    /// <summary>
    /// Government ID document URL
    /// </summary>
    public string? GovtIdUrl { get; set; }
    
    /// <summary>
    /// Introduction video URL
    /// </summary>
    public string? IntroVideoUrl { get; set; }
    
    /// <summary>
    /// Resume file URL (if uploaded)
    /// </summary>
    public string? ResumeUrl { get; set; }

    // Navigation Properties
    public User Tutor { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
