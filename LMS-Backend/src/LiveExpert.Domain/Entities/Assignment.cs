using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

// Feature 3: Homework / Assignments
public class SessionAssignment : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid TutorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? FileUrl { get; set; }

    // Navigation
    public Session Session { get; set; } = null!;
    public User Tutor { get; set; } = null!;
    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}

public class AssignmentSubmission : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? FeedbackText { get; set; }
    public DateTime? FeedbackAt { get; set; }
    public int? Grade { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    // Navigation
    public SessionAssignment Assignment { get; set; } = null!;
    public User Student { get; set; } = null!;
}
