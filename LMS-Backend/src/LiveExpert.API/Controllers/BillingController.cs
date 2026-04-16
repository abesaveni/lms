using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize(Roles = "Student")]
public class BillingController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public BillingController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid GetUserId() => _currentUser.UserId ?? Guid.Empty;

    /// <summary>Unified billing history — session bookings + course enrollments</summary>
    [HttpGet("history")]
    public async Task<IActionResult> History(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var studentId = GetUserId();

        var sessionItems = await _db.Payments
            .Where(p => p.StudentId == studentId && p.Status == PaymentStatus.Success)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new BillingItem
            {
                Id = p.Id.ToString(),
                Type = "Session",
                Title = "Session Booking",
                Amount = p.TotalAmount,
                PlatformFee = p.PlatformFee,
                Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod ?? "Razorpay",
                GatewayPaymentId = p.GatewayPaymentId,
                Date = p.CreatedAt
            })
            .ToListAsync(ct);

        // Materialize from DB first — cannot assign anonymous type to object? inside EF Core SQL query
        var rawCourseEnrollments = await _db.CourseEnrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new
            {
                e.Id,
                CourseTitle      = e.Course.Title,
                e.AmountPaid,
                e.PlatformFee,
                Status           = e.Status.ToString(),
                e.GatewayPaymentId,
                Date             = e.EnrolledAt ?? e.CreatedAt,
                e.SessionsPurchased,
                e.SessionsCompleted
            })
            .ToListAsync(ct);

        var courseItems = rawCourseEnrollments.Select(e => new BillingItem
        {
            Id               = e.Id.ToString(),
            Type             = "Course",
            Title            = e.CourseTitle,
            Amount           = e.AmountPaid,
            PlatformFee      = e.PlatformFee,
            Status           = e.Status,
            PaymentMethod    = "Razorpay",
            GatewayPaymentId = e.GatewayPaymentId,
            Date             = e.Date,
            Extra            = new { e.SessionsPurchased, e.SessionsCompleted }
        }).ToList();

        var all = sessionItems
            .Concat(courseItems)
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            data = all,
            total = sessionItems.Count + courseItems.Count,
            page,
            pageSize
        });
    }

    /// <summary>Summary stats for the student billing dashboard</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var studentId = GetUserId();

        var sessionTotal = await _db.Payments
            .Where(p => p.StudentId == studentId && p.Status == PaymentStatus.Success)
            .SumAsync(p => (decimal?)p.TotalAmount, ct) ?? 0;

        var courseTotal = await _db.CourseEnrollments
            .Where(e => e.StudentId == studentId && e.Status != EnrollmentStatus.Cancelled)
            .SumAsync(e => (decimal?)e.AmountPaid, ct) ?? 0;

        var activeEnrollments = await _db.CourseEnrollments
            .CountAsync(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active, ct);

        return Ok(new
        {
            totalSpent = sessionTotal + courseTotal,
            sessionPayments = sessionTotal,
            coursePayments = courseTotal,
            activeEnrollments
        });
    }
}

public class BillingItem
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? GatewayPaymentId { get; set; }
    public DateTime Date { get; set; }
    public object? Extra { get; set; }
}
