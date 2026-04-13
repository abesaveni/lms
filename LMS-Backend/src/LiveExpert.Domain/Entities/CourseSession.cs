using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// A single scheduled (or planned) session within a Course.
/// </summary>
public class CourseSession : BaseEntity
{
    public Guid CourseId { get; set; }
    public Guid TutorId { get; set; }

    public int SessionNumber { get; set; }          // 1-based index within the course
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TopicsCovered { get; set; }      // newline-separated topics

    // ── Scheduling ────────────────────────────────────────────────────────────
    public DateTime? ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? MeetingLink { get; set; }
    public string? RecordingUrl { get; set; }

    // ── Status ────────────────────────────────────────────────────────────────
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public DateTime? CompletedAt { get; set; }

    // ── Notes ─────────────────────────────────────────────────────────────────
    public string? TutorNotes { get; set; }
    public string? HomeworkAssigned { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public Course Course { get; set; } = null!;
    public User Tutor { get; set; } = null!;
}
