using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

public class VirtualClassroomSession : BaseEntity
{
    public Guid TutorId { get; set; }
    public string MeetingLink { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = "Active"; // Active, Ended

    // Navigation Properties
    public User Tutor { get; set; } = null!;
}




