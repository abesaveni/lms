using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Tutor earnings and payout management
/// </summary>
[Authorize(Roles = "Tutor")]
[Route("api/tutor/earnings")]
[ApiController]
public class TutorEarningsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;

    public TutorEarningsController(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Get earnings overview for current tutor
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<EarningsOverviewDto>), 200)]
    public async Task<IActionResult> GetEarningsOverview()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<EarningsOverviewDto>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var earnings = await _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value)
            .ToListAsync();

        var totalEarned = earnings.Sum(e => e.Amount);
        var pending = earnings.Where(e => e.Status == EarningStatus.Pending).Sum(e => e.Amount);
        var available = earnings.Where(e => e.Status == EarningStatus.Available).Sum(e => e.Amount);
        var paid = earnings.Where(e => e.Status == EarningStatus.Paid).Sum(e => e.Amount);

        var overview = new EarningsOverviewDto
        {
            TotalEarned = totalEarned,
            Pending = pending,
            Available = available,
            Paid = paid,
        };

        return Ok(Result<EarningsOverviewDto>.SuccessResult(overview));
    }

    /// <summary>
    /// Get earnings history
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(Result<List<EarningHistoryDto>>), 200)]
    public async Task<IActionResult> GetEarningsHistory([FromQuery] string? sourceType, [FromQuery] string? status)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<List<EarningHistoryDto>>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var query = _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value)
            .AsQueryable();

        if (!string.IsNullOrEmpty(sourceType))
        {
            query = query.Where(e => e.SourceType == sourceType);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<EarningStatus>(status, out var earningStatus))
            {
                query = query.Where(e => e.Status == earningStatus);
            }
        }

        var earnings = await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var history = earnings.Select(e => new EarningHistoryDto
        {
            Id = e.Id,
            SourceType = e.SourceType,
            SourceId = e.SourceId,
            Amount = e.Amount,
            NetAmount = e.NetAmount,
            CommissionAmount = e.CommissionAmount,
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt,
            AvailableAt = e.AvailableAt,
            PaidAt = e.PaidAt,
        }).ToList();

        return Ok(Result<List<EarningHistoryDto>>.SuccessResult(history));
    }

    /// <summary>Get earnings breakdown by source (session vs course enrollment)</summary>
    [HttpGet("breakdown")]
    public async Task<IActionResult> GetBreakdown([FromQuery] string? period = "month")
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return Unauthorized();

        var fromDate = period switch
        {
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddMonths(-1),
            "year" => DateTime.UtcNow.AddYears(-1),
            _ => DateTime.UtcNow.AddMonths(-1)
        };

        var earnings = await _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value && e.CreatedAt >= fromDate)
            .ToListAsync();

        var bySource = earnings
            .GroupBy(e => e.SourceType)
            .Select(g => new
            {
                sourceType = g.Key,
                totalAmount = g.Sum(e => e.Amount),
                netAmount = g.Sum(e => e.NetAmount),
                count = g.Count()
            });

        return Ok(new { data = bySource, period });
    }

    /// <summary>Get auto-payout settings for the tutor</summary>
    [HttpGet("auto-payout")]
    public async Task<IActionResult> GetAutoPayoutSettings()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return Unauthorized();

        var profile = await _context.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        if (profile == null) return NotFound();

        return Ok(new
        {
            schedule = profile.AutoPayoutSchedule.ToString(),
            minimumAmount = profile.AutoPayoutMinimumAmount
        });
    }

    /// <summary>Update auto-payout settings</summary>
    [HttpPut("auto-payout")]
    public async Task<IActionResult> UpdateAutoPayoutSettings([FromBody] AutoPayoutSettingsRequest req)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return Unauthorized();

        var profile = await _context.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == userId.Value);
        if (profile == null) return NotFound();

        if (!Enum.TryParse<AutoPayoutSchedule>(req.Schedule, true, out var schedule))
            return BadRequest(new { error = "Invalid schedule. Use: Disabled, Weekly, BiWeekly, Monthly" });

        profile.AutoPayoutSchedule = schedule;
        profile.AutoPayoutMinimumAmount = Math.Max(0, req.MinimumAmount);

        _context.TutorProfiles.Update(profile);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Auto-payout settings updated", schedule = schedule.ToString() });
    }
}

public class AutoPayoutSettingsRequest
{
    public string Schedule { get; set; } = "Disabled";
    public decimal MinimumAmount { get; set; } = 1000;
}

public class EarningsOverviewDto
{
    public decimal TotalEarned { get; set; }
    public decimal Pending { get; set; }
    public decimal Available { get; set; }
    public decimal Paid { get; set; }
}

public class EarningHistoryDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public decimal Amount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AvailableAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
