using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Per-subject hourly rate for a tutor (replaces single flat HourlyRate on TutorProfile).
/// </summary>
public class TutorSubjectRate : BaseEntity
{
    public Guid TutorId { get; set; }
    public Guid? SubjectId { get; set; }            // null = custom / free-text subject
    public string SubjectName { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal? TrialRate { get; set; }         // intro/trial session rate (0 = free)
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation
    public User Tutor { get; set; } = null!;
    public Subject? Subject { get; set; }
}
