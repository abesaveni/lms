using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

// Feature 2: Session notes by tutor
public class SessionNote : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid TutorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsVisibleToStudent { get; set; } = true;

    // Navigation
    public Session Session { get; set; } = null!;
    public User Tutor { get; set; } = null!;
}
