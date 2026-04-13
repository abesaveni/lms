using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin tutor verification management
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin/tutors/verification")]
[ApiController]
public class AdminTutorVerificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public AdminTutorVerificationController(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get pending tutor verifications
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(Result<List<TutorVerificationDto>>), 200)]
    public async Task<IActionResult> GetPendingVerifications()
    {
        var verifications = await _context.TutorVerifications
            .Include(v => v.Tutor)
            .ThenInclude(t => t.TutorProfile)
            .Where(v => v.Status == VerificationStatus.Pending)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();

        var verificationDtos = verifications.Select(v => new TutorVerificationDto
        {
            Id = v.Id,
            TutorId = v.TutorId,
            TutorName = $"{v.Tutor.FirstName} {v.Tutor.LastName}",
            TutorEmail = v.Tutor.Email,
            Skills = v.Tutor.TutorProfile?.Skills ?? "",
            Experience = v.Tutor.TutorProfile?.YearsOfExperience ?? 0,
            Education = v.Tutor.TutorProfile?.Education ?? "",
            Certifications = v.Tutor.TutorProfile?.Certifications ?? "",
            ResumeUrl = v.Tutor.TutorProfile?.ResumeUrl,
            IntroVideoUrl = v.Tutor.TutorProfile?.VideoIntroUrl,
            GovtIdUrl = v.Tutor.TutorProfile?.GovtIdUrl,
            SubmittedAt = v.CreatedAt,
        }).ToList();

        return Ok(Result<List<TutorVerificationDto>>.SuccessResult(verificationDtos));
    }

    /// <summary>
    /// Get verified tutor list
    /// </summary>
    [HttpGet("verified")]
    [ProducesResponseType(typeof(Result<List<VerifiedTutorDto>>), 200)]
    public async Task<IActionResult> GetVerifiedTutors()
    {
        var verifications = await _context.TutorVerifications
            .Include(v => v.Tutor)
            .Where(v => v.Status == VerificationStatus.Approved)
            .OrderByDescending(v => v.UpdatedAt != default ? v.UpdatedAt : v.CreatedAt)
            .ToListAsync();

        var results = verifications.Select(v => new VerifiedTutorDto
        {
            Id = v.TutorId,
            Name = $"{v.Tutor.FirstName} {v.Tutor.LastName}",
            Email = v.Tutor.Email,
            VerifiedAt = v.UpdatedAt != default ? v.UpdatedAt : v.CreatedAt,
            VerifiedBy = v.VerifiedBy?.ToString() ?? "System"
        }).ToList();

        return Ok(Result<List<VerifiedTutorDto>>.SuccessResult(results));
    }

    /// <summary>
    /// Approve tutor verification
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> ApproveTutor(Guid id, [FromBody] ApproveTutorRequest request)
    {
        var adminId = _currentUserService.UserId;
        if (!adminId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "Admin not authenticated"));
        }

        var verification = await _context.TutorVerifications
            .Include(v => v.Tutor)
            .ThenInclude(t => t.TutorProfile)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (verification == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Verification not found"));
        }

        if (verification.Status != VerificationStatus.Pending)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Verification is not pending"));
        }

        verification.Status = VerificationStatus.Approved;
        verification.VerifiedBy = adminId.Value;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.AdminNotes = request.Notes;
        verification.UpdatedAt = DateTime.UtcNow;

        // Update tutor profile
        if (verification.Tutor.TutorProfile != null)
        {
            verification.Tutor.TutorProfile.VerificationStatus = VerificationStatus.Approved;
            verification.Tutor.TutorProfile.VerifiedAt = DateTime.UtcNow;
            verification.Tutor.TutorProfile.VerifiedBy = adminId.Value;
            verification.Tutor.TutorProfile.IsVisible = true; // Make tutor visible in Find Tutors
            verification.Tutor.TutorProfile.IsProfileComplete = true;
            verification.Tutor.TutorProfile.OnboardingStep = 4; // Complete
        }

        await _context.SaveChangesAsync();

        // Send verification email
        await _notificationService.SendTutorVerifiedAsync(verification.Tutor, CancellationToken.None);

        return Ok(Result.SuccessResult());
    }

    /// <summary>
    /// Reject tutor verification
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> RejectTutor(Guid id, [FromBody] RejectTutorRequest request)
    {
        var adminId = _currentUserService.UserId;
        if (!adminId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "Admin not authenticated"));
        }

        var verification = await _context.TutorVerifications
            .Include(v => v.Tutor)
            .ThenInclude(t => t.TutorProfile)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (verification == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Verification not found"));
        }

        if (verification.Status != VerificationStatus.Pending)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Verification is not pending"));
        }

        verification.Status = VerificationStatus.Rejected;
        verification.VerifiedBy = adminId.Value;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.RejectionReason = request.Reason;
        verification.AdminNotes = request.Notes;
        verification.UpdatedAt = DateTime.UtcNow;

        // Update tutor profile
        if (verification.Tutor.TutorProfile != null)
        {
            verification.Tutor.TutorProfile.VerificationStatus = VerificationStatus.Rejected;
            verification.Tutor.TutorProfile.RejectionReason = request.Reason;
            verification.Tutor.TutorProfile.IsVisible = false;
        }

        await _context.SaveChangesAsync();

        // Send rejection email
        await _notificationService.SendTutorRejectedAsync(verification.Tutor, request.Reason, CancellationToken.None);

        return Ok(Result.SuccessResult());
    }
}

public class TutorVerificationDto
{
    public Guid Id { get; set; }
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string TutorEmail { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public int Experience { get; set; }
    public string Education { get; set; } = string.Empty;
    public string Certifications { get; set; } = string.Empty;
    public string? ResumeUrl { get; set; }
    public string? IntroVideoUrl { get; set; }
    public string? GovtIdUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class ApproveTutorRequest
{
    public string? Notes { get; set; }
}

public class RejectTutorRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class VerifiedTutorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
}
