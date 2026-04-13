using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// A structured course created by a tutor — contains multiple sessions with a syllabus and pricing.
/// </summary>
public class Course : BaseEntity
{
    public Guid TutorId { get; set; }

    // ── Basic Info ────────────────────────────────────────────────────────────
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }

    // ── Subject / Category ────────────────────────────────────────────────────
    public Guid? SubjectId { get; set; }
    public string? SubjectName { get; set; }        // denormalised for display without join
    public string? CategoryName { get; set; }

    // ── Metadata ──────────────────────────────────────────────────────────────
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public string Language { get; set; } = "English";
    public string? ThumbnailUrl { get; set; }
    public string? TagsJson { get; set; }           // JSON array of tags

    // ── Structure ─────────────────────────────────────────────────────────────
    public int TotalSessions { get; set; } = 1;
    public int SessionDurationMinutes { get; set; } = 60;
    public CourseDeliveryType DeliveryType { get; set; } = CourseDeliveryType.LiveOneOnOne;
    public int MaxStudentsPerBatch { get; set; } = 1;

    // ── Pricing ───────────────────────────────────────────────────────────────
    public decimal PricePerSession { get; set; }
    public decimal? BundlePrice { get; set; }       // full-course discounted price (null = no bundle)
    public bool AllowPartialBooking { get; set; } = true;
    public int MinSessionsForPartial { get; set; } = 1;
    public string? RefundPolicy { get; set; }

    // ── Trial ─────────────────────────────────────────────────────────────────
    public bool TrialAvailable { get; set; } = false;
    public int TrialDurationMinutes { get; set; } = 30;
    public decimal TrialPrice { get; set; } = 0;    // 0 = free

    // ── Content ───────────────────────────────────────────────────────────────
    public string? Prerequisites { get; set; }
    public string? MaterialsRequired { get; set; }
    public string? WhatYouWillLearn { get; set; }   // newline-separated bullet points
    public string? SyllabusJson { get; set; }       // JSON array of { sessionNo, title, topics }

    // ── Status ────────────────────────────────────────────────────────────────
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public bool IsVisible { get; set; } = false;
    public DateTime? PublishedAt { get; set; }

    // ── Denormalised Metrics ──────────────────────────────────────────────────
    public int TotalEnrollments { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public User Tutor { get; set; } = null!;
    public Subject? Subject { get; set; }
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
    public ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();
    public ICollection<TrialSession> TrialSessions { get; set; } = new List<TrialSession>();
}
