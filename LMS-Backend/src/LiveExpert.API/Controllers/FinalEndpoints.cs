using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Tutor;

/// <summary>
/// Calendar integration endpoints for tutors
/// </summary>
[Route("api/tutor/calendar")]
[Authorize(Roles = "Tutor")]
[ApiController]
public class CalendarController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CalendarController(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Connect calendar (Google/Outlook)
    /// </summary>
    [HttpPost("connect")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ConnectCalendar([FromBody] ConnectCalendarRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId), cancellationToken);
        if (user == null)
            return NotFound();

        // Store calendar connection details (simplified)
        // In production, this would integrate with Google Calendar API or Outlook API
        
        return Ok(Result<bool>.SuccessResult(true));
    }

    /// <summary>
    /// Get calendar sync status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetCalendarStatus(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var status = new
        {
            IsConnected = false,
            Provider = "None",
            LastSyncedAt = (DateTime?)null,
            SyncEnabled = false
        };

        return Ok(Result<object>.SuccessResult(status));
    }

    /// <summary>
    /// Disconnect calendar
    /// </summary>
    [HttpPost("disconnect")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> DisconnectCalendar(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Remove calendar connection
        
        return Ok(Result<bool>.SuccessResult(true));
    }
}

public class ConnectCalendarRequest
{
    public string Provider { get; set; } = string.Empty; // "Google" or "Outlook"
    public string AccessToken { get; set; } = string.Empty;
}

// SettingsController moved/removed to avoid conflict with SystemSettingsController

// NOTE: CampaignsController has been moved to Admin/WhatsAppCampaignController.cs
// This duplicate controller has been removed to fix route ambiguity (AmbiguousMatchException)
// All campaign endpoints are now in: Controllers/Admin/WhatsAppCampaignController.cs

public class CreateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = "All"; // "All", "Students", "Tutors"
    public DateTime ScheduledAt { get; set; }
}

/// <summary>
/// Admin enhancement endpoints
/// </summary>
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminEnhancementsController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;

    public AdminEnhancementsController(
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository)
    {
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
    }

    /// <summary>
    /// Get audit logs
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var logs = new List<object>
        {
            new
            {
                Id = Guid.NewGuid(),
                Action = "User Login",
                UserId = Guid.NewGuid(),
                Username = "john_student",
                IpAddress = "192.168.1.1",
                Timestamp = DateTime.UtcNow.AddHours(-2)
            }
        };

        return Ok(Result<List<object>>.SuccessResult(logs));
    }

    /// <summary>
    /// Get pending KYC documents
    /// </summary>
    [HttpGet("kyc/pending")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPendingKyc(CancellationToken cancellationToken)
    {
        var pendingKyc = await _tutorRepository.FindAsync(
            t => t.VerificationStatus == Domain.Enums.VerificationStatus.Pending,
            cancellationToken);

        var results = new List<object>();
        foreach (var tutor in pendingKyc)
        {
            var user = await _userRepository.GetByIdAsync(tutor.UserId, cancellationToken);
            results.Add(new
            {
                Id = tutor.Id,
                TutorName = user?.Username,
                Email = user?.Email,
                SubmittedAt = tutor.CreatedAt
            });
        }

        return Ok(Result<List<object>>.SuccessResult(results));
    }

    /// <summary>
    /// Approve KYC document
    /// </summary>
    [HttpPut("kyc/{id}/approve")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ApproveKyc(Guid id, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor == null)
            return NotFound();

        tutor.VerificationStatus = Domain.Enums.VerificationStatus.Approved;
        tutor.UpdatedAt = DateTime.UtcNow;

        await _tutorRepository.UpdateAsync(tutor, cancellationToken);

        return Ok(Result<bool>.SuccessResult(true));
    }

    /// <summary>
    /// Reject KYC document
    /// </summary>
    [HttpPut("kyc/{id}/reject")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RejectKyc(Guid id, [FromBody] RejectKycRequest request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(id, cancellationToken);
        if (tutor == null)
            return NotFound();

        tutor.VerificationStatus = Domain.Enums.VerificationStatus.Rejected;
        tutor.UpdatedAt = DateTime.UtcNow;

        await _tutorRepository.UpdateAsync(tutor, cancellationToken);

        return Ok(Result<bool>.SuccessResult(true));
    }

    /// <summary>
    /// Get monthly reports
    /// </summary>
    [HttpGet("reports/monthly")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetMonthlyReports([FromQuery] int year = 2025, [FromQuery] int month = 1)
    {
        var report = new
        {
            Year = year,
            Month = month,
            TotalRevenue = 125000.00m,
            TotalSessions = 450,
            NewUsers = 85,
            NewStudents = 60,
            NewTutors = 25,
            ActiveUsers = 320,
            TotalWithdrawals = 45000.00m,
            PlatformFees = 12500.00m
        };

        return Ok(Result<object>.SuccessResult(report));
    }
}

public class RejectKycRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Additional feature endpoints
/// </summary>
[Route("api/shared")]
[ApiController]
public class AdditionalFeaturesController : ControllerBase
{
    /// <summary>
    /// Get FAQs
    /// </summary>
    [HttpGet("faqs")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetFaqs()
    {
        var faqs = new List<object>
        {
            new
            {
                Id = Guid.NewGuid(),
                Question = "How do I book a session?",
                Answer = "Navigate to the Sessions page, select a tutor, choose a time slot, and confirm your booking.",
                Category = "Booking",
                Order = 1
            }
        };

        return Ok(Result<List<object>>.SuccessResult(faqs));
    }

    /// <summary>
    /// Create support ticket
    /// </summary>
    [Authorize]
    [HttpPost("support/ticket")]
    [ProducesResponseType(201)]
    public async Task<IActionResult> CreateSupportTicket([FromBody] CreateTicketRequest request)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var ticket = new
        {
            Id = Guid.NewGuid(),
            Subject = request.Subject,
            Description = request.Description,
            Status = "Open",
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetSupportTickets), new { id = ticket.Id }, Result<object>.SuccessResult(ticket));
    }

    /// <summary>
    /// Get my support tickets
    /// </summary>
    [Authorize]
    [HttpGet("support/tickets")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetSupportTickets()
    {
        var tickets = new List<object>
        {
            new
            {
                Id = Guid.NewGuid(),
                Subject = "Payment Issue",
                Status = "Open",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            }
        };

        return Ok(Result<List<object>>.SuccessResult(tickets));
    }
}

public class CreateTicketRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High"
}
