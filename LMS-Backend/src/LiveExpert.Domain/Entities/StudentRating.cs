using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

// Feature 4: Mutual rating - tutor rates student
public class StudentRating : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid TutorId { get; set; }
    public Guid StudentId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public int Punctuality { get; set; }
    public int Preparedness { get; set; }

    // Navigation
    public Session Session { get; set; } = null!;
    public User Tutor { get; set; } = null!;
    public User Student { get; set; } = null!;
}
