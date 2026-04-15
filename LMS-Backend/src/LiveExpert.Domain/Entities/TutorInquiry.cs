using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

// Feature 12: Pre-booking inquiry
public class TutorInquiry : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TutorId { get; set; }
    public Guid? SubjectId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TutorReply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public InquiryStatus Status { get; set; } = InquiryStatus.Pending;

    // Navigation
    public User Student { get; set; } = null!;
    public User Tutor { get; set; } = null!;
    public Subject? Subject { get; set; }
}
