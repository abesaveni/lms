using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LiveExpert.API.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// Request / Response DTOs
// ─────────────────────────────────────────────────────────────────────────────

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public Guid? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public string? CategoryName { get; set; }
    public string Level { get; set; } = "Beginner";
    public string Language { get; set; } = "English";
    public string? ThumbnailUrl { get; set; }
    public string[]? Tags { get; set; }
    public int TotalSessions { get; set; } = 1;
    public int SessionDurationMinutes { get; set; } = 60;
    public string DeliveryType { get; set; } = "LiveOneOnOne";
    public int MaxStudentsPerBatch { get; set; } = 1;
    public decimal PricePerSession { get; set; }
    public decimal? BundlePrice { get; set; }
    public bool AllowPartialBooking { get; set; } = true;
    public int MinSessionsForPartial { get; set; } = 1;
    public string? RefundPolicy { get; set; }
    public bool TrialAvailable { get; set; } = false;
    public int TrialDurationMinutes { get; set; } = 30;
    public decimal TrialPrice { get; set; } = 0;
    public string? Prerequisites { get; set; }
    public string? MaterialsRequired { get; set; }
    public string? WhatYouWillLearn { get; set; }
    public SyllabusItemRequest[]? Syllabus { get; set; }
}

public class SyllabusItemRequest
{
    public int SessionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Topics { get; set; }
    public string? Description { get; set; }
}

public class UpdateCourseStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class CreateCourseSessionRequest
{
    public int SessionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TopicsCovered { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? HomeworkAssigned { get; set; }
}

public class UpdateSubjectRatesRequest
{
    public SubjectRateItem[] Rates { get; set; } = [];
}

public class SubjectRateItem
{
    public Guid? SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal? TrialRate { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Course Controller
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/courses")]
public class CourseController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CourseController> _logger;

    public CourseController(ApplicationDbContext db, ICurrentUserService currentUser, ILogger<CourseController> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    private Guid GetUserId() => _currentUser.UserId ?? Guid.Empty;

    // ── Public Browse ─────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Browse(
        [FromQuery] string? subject, [FromQuery] string? level, [FromQuery] string? q,
        [FromQuery] decimal? maxPrice, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _db.Courses
            .Include(c => c.Tutor)
            .Where(c => c.Status == CourseStatus.Published && c.IsVisible)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Title.Contains(q) || (c.ShortDescription != null && c.ShortDescription.Contains(q)));
        if (!string.IsNullOrWhiteSpace(subject))
            query = query.Where(c => c.SubjectName != null && c.SubjectName.ToLower().Contains(subject.ToLower()));
        if (!string.IsNullOrWhiteSpace(level) && Enum.TryParse<CourseLevel>(level, true, out var lvl))
            query = query.Where(c => c.Level == lvl);
        if (maxPrice.HasValue)
            query = query.Where(c => c.PricePerSession <= maxPrice.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.TotalEnrollments)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id, c.Title, c.ShortDescription, c.SubjectName, c.CategoryName,
                Level = c.Level.ToString(), c.Language, c.ThumbnailUrl, c.TotalSessions, c.SessionDurationMinutes,
                DeliveryType = c.DeliveryType.ToString(),
                c.PricePerSession, c.BundlePrice, c.TrialAvailable, c.TrialPrice,
                c.TrialDurationMinutes, c.AverageRating, c.TotalReviews, c.TotalEnrollments,
                TutorName = c.Tutor.FirstName + " " + c.Tutor.LastName, c.TutorId
            })
            .ToListAsync(ct);

        return Ok(new { data = items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var course = await _db.Courses
            .Include(c => c.Tutor)
            .Include(c => c.CourseSessions.OrderBy(s => s.SessionNumber))
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course == null) return NotFound();

        var userId = GetUserId();
        if (course.Status != CourseStatus.Published && course.TutorId != userId)
            return NotFound();

        var tutor = await _db.TutorProfiles.FirstOrDefaultAsync(t => t.UserId == course.TutorId, ct);

        return Ok(new
        {
            course.Id, course.Title, course.ShortDescription, course.FullDescription,
            course.SubjectName, course.CategoryName, Level = course.Level.ToString(),
            course.Language, course.ThumbnailUrl, course.TotalSessions, course.SessionDurationMinutes,
            DeliveryType = course.DeliveryType.ToString(), course.MaxStudentsPerBatch,
            course.PricePerSession, course.BundlePrice, course.AllowPartialBooking,
            course.MinSessionsForPartial, course.RefundPolicy,
            course.TrialAvailable, course.TrialPrice, course.TrialDurationMinutes,
            course.Prerequisites, course.MaterialsRequired, course.WhatYouWillLearn,
            course.SyllabusJson, course.TagsJson,
            course.AverageRating, course.TotalReviews, course.TotalEnrollments,
            Status = course.Status.ToString(), course.PublishedAt,
            Tutor = new
            {
                course.TutorId,
                Name = course.Tutor.FirstName + " " + course.Tutor.LastName,
                tutor?.Bio, tutor?.Headline, tutor?.AverageRating,
                tutor?.TotalSessions, tutor?.YearsOfExperience
            },
            Sessions = course.CourseSessions.Select(s => new
            {
                s.SessionNumber, s.Title, s.Description, s.TopicsCovered,
                s.ScheduledAt, s.DurationMinutes, Status = s.Status.ToString()
            })
        });
    }

    // ── Tutor CRUD ────────────────────────────────────────────────────────────

    [HttpGet("my")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> MyCourses(CancellationToken ct)
    {
        var tutorId = GetUserId();
        var courses = await _db.Courses
            .Where(c => c.TutorId == tutorId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id, c.Title, c.ShortDescription, c.SubjectName,
                Level = c.Level.ToString(), Status = c.Status.ToString(),
                c.TotalSessions, c.PricePerSession, c.BundlePrice,
                c.TotalEnrollments, c.AverageRating, c.TotalReviews,
                c.IsVisible, c.PublishedAt, c.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(new { data = courses });
    }

    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest req, CancellationToken ct)
    {
        var tutorId = GetUserId();
        if (tutorId == Guid.Empty) return Unauthorized();
        if (string.IsNullOrWhiteSpace(req.Title)) return BadRequest(new { error = "Title is required" });
        if (req.PricePerSession <= 0) return BadRequest(new { error = "Price per session must be > 0" });

        if (!Enum.TryParse<CourseLevel>(req.Level, true, out var level)) level = CourseLevel.Beginner;
        if (!Enum.TryParse<CourseDeliveryType>(req.DeliveryType, true, out var delivery)) delivery = CourseDeliveryType.LiveOneOnOne;

        var course = new Course
        {
            Id = Guid.NewGuid(), TutorId = tutorId,
            Title = req.Title.Trim(), ShortDescription = req.ShortDescription?.Trim(),
            FullDescription = req.FullDescription?.Trim(), SubjectId = req.SubjectId,
            SubjectName = req.SubjectName?.Trim(), CategoryName = req.CategoryName?.Trim(),
            Level = level, Language = req.Language, ThumbnailUrl = req.ThumbnailUrl,
            TagsJson = req.Tags != null ? JsonSerializer.Serialize(req.Tags) : null,
            TotalSessions = Math.Max(1, req.TotalSessions), SessionDurationMinutes = req.SessionDurationMinutes,
            DeliveryType = delivery, MaxStudentsPerBatch = Math.Max(1, req.MaxStudentsPerBatch),
            PricePerSession = req.PricePerSession, BundlePrice = req.BundlePrice,
            AllowPartialBooking = req.AllowPartialBooking, MinSessionsForPartial = req.MinSessionsForPartial,
            RefundPolicy = req.RefundPolicy, TrialAvailable = req.TrialAvailable,
            TrialDurationMinutes = req.TrialDurationMinutes, TrialPrice = req.TrialPrice,
            Prerequisites = req.Prerequisites, MaterialsRequired = req.MaterialsRequired,
            WhatYouWillLearn = req.WhatYouWillLearn,
            SyllabusJson = req.Syllabus != null ? JsonSerializer.Serialize(req.Syllabus) : null,
            Status = CourseStatus.Draft, IsVisible = false
        };

        _db.Courses.Add(course);

        if (req.Syllabus != null)
        {
            foreach (var item in req.Syllabus)
            {
                _db.CourseSessions.Add(new CourseSession
                {
                    Id = Guid.NewGuid(), CourseId = course.Id, TutorId = tutorId,
                    SessionNumber = item.SessionNumber, Title = item.Title,
                    Description = item.Description, TopicsCovered = item.Topics,
                    DurationMinutes = req.SessionDurationMinutes, Status = SessionStatus.Scheduled
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Tutor {TutorId} created course {CourseId}: {Title}", tutorId, course.Id, course.Title);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, new { courseId = course.Id, message = "Course created" });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCourseRequest req, CancellationToken ct)
    {
        var tutorId = GetUserId();
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.TutorId == tutorId, ct);
        if (course == null) return NotFound();

        if (!Enum.TryParse<CourseLevel>(req.Level, true, out var level)) level = CourseLevel.Beginner;
        if (!Enum.TryParse<CourseDeliveryType>(req.DeliveryType, true, out var delivery)) delivery = CourseDeliveryType.LiveOneOnOne;

        course.Title = req.Title.Trim(); course.ShortDescription = req.ShortDescription?.Trim();
        course.FullDescription = req.FullDescription?.Trim(); course.SubjectId = req.SubjectId;
        course.SubjectName = req.SubjectName?.Trim(); course.CategoryName = req.CategoryName?.Trim();
        course.Level = level; course.Language = req.Language;
        course.ThumbnailUrl = req.ThumbnailUrl ?? course.ThumbnailUrl;
        course.TagsJson = req.Tags != null ? JsonSerializer.Serialize(req.Tags) : course.TagsJson;
        course.TotalSessions = Math.Max(1, req.TotalSessions); course.SessionDurationMinutes = req.SessionDurationMinutes;
        course.DeliveryType = delivery; course.MaxStudentsPerBatch = Math.Max(1, req.MaxStudentsPerBatch);
        course.PricePerSession = req.PricePerSession; course.BundlePrice = req.BundlePrice;
        course.AllowPartialBooking = req.AllowPartialBooking; course.MinSessionsForPartial = req.MinSessionsForPartial;
        course.RefundPolicy = req.RefundPolicy; course.TrialAvailable = req.TrialAvailable;
        course.TrialDurationMinutes = req.TrialDurationMinutes; course.TrialPrice = req.TrialPrice;
        course.Prerequisites = req.Prerequisites; course.MaterialsRequired = req.MaterialsRequired;
        course.WhatYouWillLearn = req.WhatYouWillLearn;
        course.SyllabusJson = req.Syllabus != null ? JsonSerializer.Serialize(req.Syllabus) : course.SyllabusJson;

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Course updated" });
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCourseStatusRequest req, CancellationToken ct)
    {
        var tutorId = GetUserId();
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.TutorId == tutorId, ct);
        if (course == null) return NotFound();
        if (!Enum.TryParse<CourseStatus>(req.Status, true, out var newStatus))
            return BadRequest(new { error = "Invalid status. Use: Draft, Published, Paused, Archived" });
        if (newStatus == CourseStatus.Published && course.PricePerSession <= 0)
            return BadRequest(new { error = "Set a price before publishing" });

        course.Status = newStatus;
        course.IsVisible = newStatus == CourseStatus.Published;
        if (newStatus == CourseStatus.Published && course.PublishedAt == null)
            course.PublishedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = $"Course {newStatus.ToString().ToLower()}" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var tutorId = GetUserId();
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.TutorId == tutorId, ct);
        if (course == null) return NotFound();
        if (course.TotalEnrollments > 0)
            return BadRequest(new { error = "Cannot delete a course with enrollments. Archive it instead." });

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Course deleted" });
    }

    // ── Course Sessions ───────────────────────────────────────────────────────

    [HttpPost("{id:guid}/sessions")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> UpsertCourseSession(Guid id, [FromBody] CreateCourseSessionRequest req, CancellationToken ct)
    {
        var tutorId = GetUserId();
        if (!await _db.Courses.AnyAsync(c => c.Id == id && c.TutorId == tutorId, ct)) return NotFound();

        var existing = await _db.CourseSessions
            .FirstOrDefaultAsync(s => s.CourseId == id && s.SessionNumber == req.SessionNumber, ct);

        if (existing != null)
        {
            existing.Title = req.Title; existing.Description = req.Description;
            existing.TopicsCovered = req.TopicsCovered; existing.ScheduledAt = req.ScheduledAt;
            existing.DurationMinutes = req.DurationMinutes; existing.HomeworkAssigned = req.HomeworkAssigned;
        }
        else
        {
            _db.CourseSessions.Add(new CourseSession
            {
                Id = Guid.NewGuid(), CourseId = id, TutorId = tutorId,
                SessionNumber = req.SessionNumber, Title = req.Title,
                Description = req.Description, TopicsCovered = req.TopicsCovered,
                ScheduledAt = req.ScheduledAt, DurationMinutes = req.DurationMinutes,
                HomeworkAssigned = req.HomeworkAssigned, Status = SessionStatus.Scheduled
            });
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Session saved" });
    }

    [HttpGet("{id:guid}/sessions")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GetCourseSessions(Guid id, CancellationToken ct)
    {
        var tutorId = GetUserId();
        if (!await _db.Courses.AnyAsync(c => c.Id == id && c.TutorId == tutorId, ct)) return NotFound();

        var sessions = await _db.CourseSessions
            .Where(s => s.CourseId == id)
            .OrderBy(s => s.SessionNumber)
            .ToListAsync(ct);
        return Ok(new { data = sessions });
    }

    // ── Subject Rates ─────────────────────────────────────────────────────────

    [HttpGet("subject-rates")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GetSubjectRates(CancellationToken ct)
    {
        var tutorId = GetUserId();
        var rates = await _db.TutorSubjectRates
            .Where(r => r.TutorId == tutorId && r.IsActive)
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync(ct);
        return Ok(new { data = rates });
    }

    [HttpPut("subject-rates")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> UpdateSubjectRates([FromBody] UpdateSubjectRatesRequest req, CancellationToken ct)
    {
        var tutorId = GetUserId();
        var existing = await _db.TutorSubjectRates.Where(r => r.TutorId == tutorId).ToListAsync(ct);
        _db.TutorSubjectRates.RemoveRange(existing);

        for (int i = 0; i < req.Rates.Length; i++)
        {
            var item = req.Rates[i];
            if (string.IsNullOrWhiteSpace(item.SubjectName) || item.HourlyRate <= 0) continue;
            _db.TutorSubjectRates.Add(new TutorSubjectRate
            {
                Id = Guid.NewGuid(), TutorId = tutorId, SubjectId = item.SubjectId,
                SubjectName = item.SubjectName.Trim(), HourlyRate = item.HourlyRate,
                TrialRate = item.TrialRate, IsActive = true, DisplayOrder = i
            });
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Subject rates updated" });
    }

    // ── Public: Tutor's courses ───────────────────────────────────────────────

    [HttpGet("by-tutor/{tutorId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> ByTutor(Guid tutorId, CancellationToken ct)
    {
        var courses = await _db.Courses
            .Where(c => c.TutorId == tutorId && c.Status == CourseStatus.Published && c.IsVisible)
            .OrderByDescending(c => c.TotalEnrollments)
            .Select(c => new
            {
                c.Id, c.Title, c.ShortDescription, c.SubjectName, Level = c.Level.ToString(),
                c.ThumbnailUrl, c.TotalSessions, c.SessionDurationMinutes,
                c.PricePerSession, c.BundlePrice, c.TrialAvailable, c.AverageRating,
                c.TotalReviews, c.TotalEnrollments
            })
            .ToListAsync(ct);

        var rates = await _db.TutorSubjectRates
            .Where(r => r.TutorId == tutorId && r.IsActive)
            .OrderBy(r => r.DisplayOrder)
            .Select(r => new { r.SubjectName, r.HourlyRate, r.TrialRate })
            .ToListAsync(ct);

        return Ok(new { courses, subjectRates = rates });
    }
}
