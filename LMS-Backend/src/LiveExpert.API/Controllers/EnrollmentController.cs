using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers;

public class CreateEnrollmentOrderRequest
{
    public Guid CourseId { get; set; }
    public string EnrollmentType { get; set; } = "Full";
    public int SessionsToPurchase { get; set; } = 0;
}

public class VerifyEnrollmentPaymentRequest
{
    public Guid CourseId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
    public string EnrollmentType { get; set; } = "Full";
    public int SessionsPurchased { get; set; }
    public decimal AmountPaid { get; set; }
}

public class BookTrialRequest
{
    public Guid TutorId { get; set; }
    public Guid? CourseId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _config;
    private readonly ILogger<EnrollmentController> _logger;

    public EnrollmentController(
        ApplicationDbContext db, ICurrentUserService currentUser,
        IPaymentService paymentService, IConfiguration config,
        ILogger<EnrollmentController> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _config = config;
        _logger = logger;
    }

    private Guid GetUserId() => _currentUser.UserId ?? Guid.Empty;

    // ── Create order ──────────────────────────────────────────────────────────

    [HttpPost("create-order")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateEnrollmentOrderRequest req, CancellationToken ct)
    {
        var studentId = GetUserId();
        if (studentId == Guid.Empty) return Unauthorized();

        var course = await _db.Courses.FirstOrDefaultAsync(
            c => c.Id == req.CourseId && c.Status == CourseStatus.Published, ct);
        if (course == null) return NotFound(new { error = "Course not found" });

        var existing = await _db.CourseEnrollments.AnyAsync(
            e => e.CourseId == req.CourseId && e.StudentId == studentId && e.Status == EnrollmentStatus.Active, ct);
        if (existing) return BadRequest(new { error = "You are already enrolled in this course" });

        bool isFull = !Enum.TryParse<EnrollmentType>(req.EnrollmentType, true, out var eType) || eType == EnrollmentType.Full;
        decimal amount;
        int sessions;
        if (isFull && course.BundlePrice.HasValue)
        {
            amount = course.BundlePrice.Value;
            sessions = course.TotalSessions;
        }
        else
        {
            sessions = isFull ? course.TotalSessions : Math.Max(req.SessionsToPurchase, course.MinSessionsForPartial);
            amount = course.PricePerSession * sessions;
        }

        var (orderId, keyId) = await _paymentService.CreateOrderAsync(amount, "INR",
            new Dictionary<string, string>
            {
                ["courseId"] = course.Id.ToString(),
                ["studentId"] = studentId.ToString(),
                ["sessions"] = sessions.ToString(),
                ["type"] = isFull ? "Full" : "Partial"
            });

        return Ok(new
        {
            orderId, keyId,
            amount = (int)(amount * 100),
            currency = "INR",
            courseTitle = course.Title,
            sessionsPurchased = sessions,
            enrollmentType = isFull ? "Full" : "Partial",
            description = $"{course.Title} — {sessions} session{(sessions > 1 ? "s" : "")}"
        });
    }

    [HttpPost("verify-payment")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyEnrollmentPaymentRequest req, CancellationToken ct)
    {
        var studentId = GetUserId();
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == req.CourseId, ct);
        if (course == null) return NotFound(new { error = "Course not found" });

        var isValid = await _paymentService.VerifyPaymentSignatureAsync(
            req.RazorpayOrderId, req.RazorpayPaymentId, req.RazorpaySignature);
        if (!isValid) return BadRequest(new { error = "Payment verification failed. Invalid signature." });

        var feeFixed = _config.GetValue<decimal>("AppSettings:PlatformFeeFixed", 100);
        var feePercent = _config.GetValue<decimal>("AppSettings:PlatformFeePercentage", 0);
        var feeEnabled = _config.GetValue<bool>("AppSettings:PlatformFeeEnabled", true);
        var platformFee = feeEnabled ? (feePercent > 0 ? req.AmountPaid * feePercent / 100 : feeFixed) : 0;

        var enrollment = new CourseEnrollment
        {
            Id = Guid.NewGuid(), CourseId = req.CourseId, StudentId = studentId,
            EnrollmentType = Enum.TryParse<EnrollmentType>(req.EnrollmentType, true, out var et) ? et : EnrollmentType.Full,
            SessionsPurchased = req.SessionsPurchased, SessionsCompleted = 0,
            AmountPaid = req.AmountPaid, PlatformFee = platformFee,
            TutorEarningAmount = req.AmountPaid - platformFee,
            GatewayOrderId = req.RazorpayOrderId, GatewayPaymentId = req.RazorpayPaymentId,
            GatewaySignature = req.RazorpaySignature,
            Status = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(6)
        };
        _db.CourseEnrollments.Add(enrollment);

        var releaseDelay = _config.GetValue<int>("AppSettings:SessionCreditReleaseDelayDays", 5);
        _db.TutorEarnings.Add(new TutorEarning
        {
            Id = Guid.NewGuid(), TutorId = course.TutorId,
            SourceType = "CourseEnrollment", SourceId = enrollment.Id,
            Amount = req.AmountPaid, CommissionPercentage = feePercent,
            CommissionAmount = platformFee, Status = EarningStatus.Pending,
            AvailableAt = DateTime.UtcNow.AddDays(releaseDelay)
        });

        course.TotalEnrollments++;
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Student {StudentId} enrolled in course {CourseId}", studentId, req.CourseId);

        return Ok(new
        {
            enrollmentId = enrollment.Id,
            message = "Enrollment confirmed!",
            sessionsPurchased = enrollment.SessionsPurchased,
            expiresAt = enrollment.ExpiresAt
        });
    }

    // ── Student enrollments ───────────────────────────────────────────────────

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyEnrollments(CancellationToken ct)
    {
        var studentId = GetUserId();
        var enrollments = await _db.CourseEnrollments
            .Include(e => e.Course).ThenInclude(c => c.Tutor)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new
            {
                e.Id, e.CourseId,
                CourseTitle = e.Course.Title, CourseThumbnail = e.Course.ThumbnailUrl,
                SubjectName = e.Course.SubjectName,
                TutorName = e.Course.Tutor.FirstName + " " + e.Course.Tutor.LastName,
                EnrollmentType = e.EnrollmentType.ToString(),
                e.SessionsPurchased, e.SessionsCompleted, SessionsRemaining = e.SessionsRemaining,
                e.AmountPaid, Status = e.Status.ToString(),
                e.EnrolledAt, e.ExpiresAt, e.CompletedAt,
                ProgressPercent = e.SessionsPurchased > 0 ? (int)((double)e.SessionsCompleted / e.SessionsPurchased * 100) : 0
            })
            .ToListAsync(ct);

        return Ok(new { data = enrollments });
    }

    [HttpGet("check/{courseId:guid}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CheckEnrollment(Guid courseId, CancellationToken ct)
    {
        var studentId = GetUserId();
        var enrollment = await _db.CourseEnrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId && e.Status == EnrollmentStatus.Active, ct);

        return Ok(new
        {
            isEnrolled = enrollment != null,
            enrollment = enrollment == null ? null : new
            {
                enrollment.Id, enrollment.SessionsPurchased, enrollment.SessionsCompleted,
                SessionsRemaining = enrollment.SessionsRemaining, enrollment.ExpiresAt
            }
        });
    }

    // ── Tutor: course enrollments ─────────────────────────────────────────────

    [HttpGet("course/{courseId:guid}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> CourseEnrollments(Guid courseId, CancellationToken ct)
    {
        var tutorId = GetUserId();
        var enrollments = await _db.CourseEnrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId && e.Course.TutorId == tutorId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new
            {
                e.Id, e.StudentId,
                StudentName = e.Student.FirstName + " " + e.Student.LastName,
                StudentEmail = e.Student.Email,
                EnrollmentType = e.EnrollmentType.ToString(),
                e.SessionsPurchased, e.SessionsCompleted,
                e.AmountPaid, e.TutorEarningAmount,
                Status = e.Status.ToString(), e.EnrolledAt, e.ExpiresAt
            })
            .ToListAsync(ct);

        return Ok(new { data = enrollments, total = enrollments.Count });
    }

    // ── Trial sessions ────────────────────────────────────────────────────────

    [HttpPost("trial")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> BookTrial([FromBody] BookTrialRequest req, CancellationToken ct)
    {
        var studentId = GetUserId();
        var hasExisting = await _db.TrialSessions
            .AnyAsync(t => t.TutorId == req.TutorId && t.StudentId == studentId, ct);
        if (hasExisting) return BadRequest(new { error = "You have already booked a trial with this tutor." });

        Course? course = req.CourseId.HasValue
            ? await _db.Courses.FirstOrDefaultAsync(c => c.Id == req.CourseId.Value, ct)
            : null;

        var trial = new TrialSession
        {
            Id = Guid.NewGuid(), TutorId = req.TutorId, StudentId = studentId,
            CourseId = req.CourseId, ScheduledAt = req.ScheduledAt,
            DurationMinutes = course?.TrialDurationMinutes ?? 30,
            Price = course?.TrialPrice ?? 0,
            Status = TrialSessionStatus.Pending
        };

        _db.TrialSessions.Add(trial);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            trialId = trial.Id, price = trial.Price,
            message = trial.Price == 0
                ? "Free trial booked. The tutor will confirm your slot."
                : $"Trial at ₹{trial.Price}. Complete payment to confirm."
        });
    }

    [HttpGet("trials/my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyTrials(CancellationToken ct)
    {
        var studentId = GetUserId();
        var trials = await _db.TrialSessions
            .Include(t => t.Tutor).Include(t => t.Course)
            .Where(t => t.StudentId == studentId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id, t.TutorId,
                TutorName = t.Tutor.FirstName + " " + t.Tutor.LastName,
                CourseTitle = t.Course != null ? t.Course.Title : null,
                t.ScheduledAt, t.DurationMinutes, t.Price,
                Status = t.Status.ToString(), t.ConvertedToEnrollment
            })
            .ToListAsync(ct);

        return Ok(new { data = trials });
    }

    [HttpGet("trials/incoming")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> IncomingTrials(CancellationToken ct)
    {
        var tutorId = GetUserId();
        var trials = await _db.TrialSessions
            .Include(t => t.Student).Include(t => t.Course)
            .Where(t => t.TutorId == tutorId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id, t.StudentId,
                StudentName = t.Student.FirstName + " " + t.Student.LastName,
                StudentEmail = t.Student.Email,
                CourseTitle = t.Course != null ? t.Course.Title : null,
                t.ScheduledAt, t.DurationMinutes, t.Price,
                Status = t.Status.ToString(), t.ConvertedToEnrollment
            })
            .ToListAsync(ct);

        return Ok(new { data = trials });
    }
}
