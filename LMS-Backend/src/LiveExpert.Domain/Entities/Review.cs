using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class Review : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid TutorId { get; set; }
    public Guid StudentId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsPublic { get; set; } = true;
    public string? TutorResponse { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Navigation Properties
    public Session Session { get; set; } = null!;
    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("TutorId")]
    [System.ComponentModel.DataAnnotations.Schema.InverseProperty("ReceivedReviews")]
    public User Tutor { get; set; } = null!;
    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("StudentId")]
    [System.ComponentModel.DataAnnotations.Schema.InverseProperty("GivenReviews")]
    public User Student { get; set; } = null!;
}

public class Dispute : BaseEntity
{
    public Guid RaisedBy { get; set; }
    public DisputeType DisputeType { get; set; }
    public Guid? RelatedToId { get; set; }
    public string? RelatedToType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }
    public Priority Priority { get; set; }
    public Guid? AssignedTo { get; set; }
    public string? Resolution { get; set; }
    public string? AttachmentUrls { get; set; } // JSON array
    public DateTime? ResolvedAt { get; set; }

    // Navigation Properties
    public User RaisedByUser { get; set; } = null!;
    public User? AssignedToUser { get; set; }
}

public class KYCDocument : BaseEntity
{
    public Guid UserId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty; // Encrypted
    public string? DocumentUrl { get; set; }
    public string? FrontImageUrl { get; set; }
    public string? BackImageUrl { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
