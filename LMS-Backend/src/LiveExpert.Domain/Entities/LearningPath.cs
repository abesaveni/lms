using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

// Feature 14: Learning paths
public class LearningPath : BaseEntity
{
    public Guid TutorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? SubjectId { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsPublished { get; set; } = false;
    public string? ThumbnailUrl { get; set; }

    // Navigation
    public User Tutor { get; set; } = null!;
    public Subject? Subject { get; set; }
    public ICollection<LearningPathStep> Steps { get; set; } = new List<LearningPathStep>();
    public ICollection<LearningPathEnrollment> Enrollments { get; set; } = new List<LearningPathEnrollment>();
}

public class LearningPathStep : BaseEntity
{
    public Guid LearningPathId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }

    // Navigation
    public LearningPath LearningPath { get; set; } = null!;
}

public class LearningPathEnrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid LearningPathId { get; set; }
    public int CompletedSteps { get; set; } = 0;
    public int CurrentStep { get; set; } = 1;
    public DateTime EnrolledAt { get; set; }
    public LearningEnrollmentStatus Status { get; set; } = LearningEnrollmentStatus.Active;

    // Navigation
    public User Student { get; set; } = null!;
    public LearningPath LearningPath { get; set; } = null!;
}
