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
/// Admin payout management
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin/payouts")]
[ApiController]
public class AdminPayoutsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;

    public AdminPayoutsController(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Platform revenue summary — 2% fees from sessions/courses + ₹99 subscriptions
    /// </summary>
    [HttpGet("revenue-summary")]
    public async Task<IActionResult> RevenueSummary([FromQuery] string? period = "all")
    {
        var fromDate = period switch
        {
            "today"  => DateTime.UtcNow.Date,
            "week"   => DateTime.UtcNow.AddDays(-7),
            "month"  => DateTime.UtcNow.AddMonths(-1),
            "year"   => DateTime.UtcNow.AddYears(-1),
            _        => DateTime.MinValue
        };

        // 2% platform fees from session + course earnings
        var earningsQuery = _context.TutorEarnings
            .Where(e => e.CreatedAt >= fromDate);

        var sessionFees  = await earningsQuery.Where(e => e.SourceType == "Session")
            .SumAsync(e => (decimal?)e.CommissionAmount) ?? 0m;
        var courseFees   = await earningsQuery.Where(e => e.SourceType == "CourseEnrollment")
            .SumAsync(e => (decimal?)e.CommissionAmount) ?? 0m;

        // ₹99 platform subscriptions
        var subscriptionRevenue = await _context.PlatformFeePayments
            .Where(p => p.CreatedAt >= fromDate)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var subscriptionCount = await _context.PlatformFeePayments
            .CountAsync(p => p.CreatedAt >= fromDate);

        // Payout totals
        var totalPaidOut = await _context.PayoutRequests
            .Where(p => p.Status == PayoutStatus.Paid && p.ProcessedAt >= fromDate)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var pendingPayouts = await _context.PayoutRequests
            .CountAsync(p => p.Status == PayoutStatus.Pending);

        // Per-tutor breakdown
        var tutorBreakdown = await _context.TutorEarnings
            .Where(e => e.CreatedAt >= fromDate)
            .GroupBy(e => e.TutorId)
            .Select(g => new
            {
                tutorId       = g.Key,
                grossEarnings = g.Sum(e => e.Amount),
                platformFees  = g.Sum(e => e.CommissionAmount),
                netEarnings   = g.Sum(e => e.Amount - e.CommissionAmount),
                count         = g.Count()
            })
            .OrderByDescending(x => x.platformFees)
            .Take(20)
            .ToListAsync();

        return Ok(new
        {
            period,
            totalPlatformRevenue = sessionFees + courseFees + subscriptionRevenue,
            breakdown = new
            {
                sessionCommissions   = sessionFees,
                courseCommissions    = courseFees,
                subscriptions        = subscriptionRevenue,
                subscriptionCount
            },
            payouts = new
            {
                totalPaidOut,
                pendingCount = pendingPayouts
            },
            netRetained = sessionFees + courseFees + subscriptionRevenue - totalPaidOut,
            tutorBreakdown
        });
    }

    /// <summary>
    /// Get all payout requests with optional status filter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPayouts([FromQuery] string? status)
    {
        var query = _context.PayoutRequests
            .Include(p => p.Tutor)
            .Include(p => p.BankAccount)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PayoutStatus>(status, out var ps))
            query = query.Where(p => p.Status == ps);

        // Materialize from DB first (cannot call _encryptionService inside EF Core LINQ projection)
        var rawPayouts = await query
            .OrderByDescending(p => p.RequestedAt)
            .Select(p => new
            {
                p.Id,
                p.TutorId,
                TutorFirstName = p.Tutor.FirstName,
                TutorLastName  = p.Tutor.LastName,
                TutorEmail     = p.Tutor.Email,
                p.Amount,
                Status         = p.Status.ToString(),
                p.RequestedAt,
                p.ProcessedAt,
                p.AdminNotes,
                p.TransactionReference,
                BankAccountHolderName = p.BankAccount != null ? p.BankAccount.AccountHolderName : null,
                BankName              = p.BankAccount != null ? p.BankAccount.BankName : null,
                IfscCode              = p.BankAccount != null ? p.BankAccount.IFSCCode : null,
                EncryptedAccountNumber = p.BankAccount != null ? p.BankAccount.AccountNumber : null,
            })
            .ToListAsync();

        // Decrypt account numbers in-memory
        var payouts = rawPayouts.Select(p => new
        {
            p.Id,
            p.TutorId,
            TutorName  = $"{p.TutorFirstName} {p.TutorLastName}".Trim(),
            p.TutorEmail,
            p.Amount,
            p.Status,
            p.RequestedAt,
            p.ProcessedAt,
            p.AdminNotes,
            p.TransactionReference,
            BankAccount = p.EncryptedAccountNumber != null ? new
            {
                AccountHolderName = p.BankAccountHolderName,
                BankName          = p.BankName,
                IfscCode          = p.IfscCode,
                AccountNumber     = _encryptionService.Decrypt(p.EncryptedAccountNumber)
            } : (object?)null
        }).ToList();

        return Ok(new { data = payouts, total = payouts.Count });
    }

    /// <summary>
    /// Get pending payout requests
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(Result<List<AdminPayoutRequestDto>>), 200)]
    public async Task<IActionResult> GetPendingPayouts()
    {
        var payouts = await _context.PayoutRequests
            .Include(p => p.Tutor)
            .Include(p => p.BankAccount)
            .Where(p => p.Status == PayoutStatus.Pending)
            .OrderBy(p => p.RequestedAt)
            .ToListAsync();

        var payoutDtos = payouts.Select(p => new AdminPayoutRequestDto
        {
            Id = p.Id,
            TutorId = p.TutorId,
            TutorName = $"{p.Tutor?.FirstName} {p.Tutor?.LastName}".Trim(),
            TutorEmail = p.Tutor?.Email ?? "",
            Amount = p.Amount,
            RequestedAt = p.RequestedAt,
            BankAccount = p.BankAccount != null ? new BankAccountDetailsDto
            {
                AccountHolderName = p.BankAccount.AccountHolderName,
                AccountNumber = _encryptionService.Decrypt(p.BankAccount.AccountNumber),
                BankName = p.BankAccount.BankName,
                IfscCode = p.BankAccount.IFSCCode,
                BranchName = p.BankAccount.BranchName,
                AccountType = p.BankAccount.AccountType.ToString(),
            } : new BankAccountDetailsDto { AccountHolderName = "N/A", BankName = "N/A", IfscCode = "N/A" },
            EarningsHistory = _context.TutorEarnings
                .Where(e => e.TutorId == p.TutorId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .Select(e => new EarningSummaryDto
                {
                    Amount = e.Amount,
                    NetAmount = e.Amount - e.CommissionAmount, // NetAmount is computed, use inline arithmetic
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt,
                })
                .ToList(),
        }).ToList();

        return Ok(Result<List<AdminPayoutRequestDto>>.SuccessResult(payoutDtos));
    }

    /// <summary>
    /// Approve payout request
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> ApprovePayout(Guid id, [FromBody] ApprovePayoutRequest request)
    {
        var adminId = _currentUserService.UserId;
        if (!adminId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "Admin not authenticated"));
        }

        var payout = await _context.PayoutRequests
            .Include(p => p.Tutor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payout == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Payout request not found"));
        }

        if (payout.Status != PayoutStatus.Pending)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Payout request is not pending"));
        }

        // Mark available earnings as paid
        var availableEarnings = await _context.TutorEarnings
            .Where(e => e.TutorId == payout.TutorId && 
                       e.Status == EarningStatus.Available)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        decimal remainingAmount = payout.Amount;
        foreach (var earning in availableEarnings)
        {
            if (remainingAmount <= 0) break;

            if (earning.NetAmount <= remainingAmount)
            {
                earning.Status = EarningStatus.Paid;
                earning.PaidAt = DateTime.UtcNow;
                earning.PayoutRequestId = payout.Id;
                remainingAmount -= earning.NetAmount;
            }
            else
            {
                // Partial payment - create new earning record for remaining
                var paidAmount = remainingAmount;
                var remainingEarning = earning.NetAmount - paidAmount;
                
                earning.Status = EarningStatus.Paid;
                earning.PaidAt = DateTime.UtcNow;
                earning.PayoutRequestId = payout.Id;
                earning.Amount = paidAmount; // Adjust for partial
                
                // Create new earning for remaining (simplified - in production, handle differently)
                remainingAmount = 0;
            }
        }

        payout.Status = PayoutStatus.Approved;
        payout.ProcessedBy = adminId.Value;
        payout.ProcessedAt = DateTime.UtcNow;
        payout.AdminNotes = request.Notes;
        payout.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Send email notification to tutor

        return Ok(Result.SuccessResult());
    }

    /// <summary>
    /// Reject payout request
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> RejectPayout(Guid id, [FromBody] RejectPayoutRequest request)
    {
        var adminId = _currentUserService.UserId;
        if (!adminId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "Admin not authenticated"));
        }

        var payout = await _context.PayoutRequests
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payout == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Payout request not found"));
        }

        if (payout.Status != PayoutStatus.Pending)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Payout request is not pending"));
        }

        payout.Status = PayoutStatus.Rejected;
        payout.ProcessedBy = adminId.Value;
        payout.ProcessedAt = DateTime.UtcNow;
        payout.AdminNotes = request.Reason;
        payout.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Send email notification to tutor

        return Ok(Result.SuccessResult());
    }

    /// <summary>
    /// Mark payout as paid (after manual transfer)
    /// </summary>
    [HttpPost("{id}/mark-paid")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> MarkPayoutAsPaid(Guid id, [FromBody] MarkPaidRequest request)
    {
        var adminId = _currentUserService.UserId;
        if (!adminId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "Admin not authenticated"));
        }

        var payout = await _context.PayoutRequests
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payout == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Payout request not found"));
        }

        if (payout.Status != PayoutStatus.Approved)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Payout must be approved first"));
        }

        payout.Status = PayoutStatus.Paid;
        payout.TransactionReference = request.TransactionReference;
        payout.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Send email notification to tutor

        return Ok(Result.SuccessResult());
    }
}

public class AdminPayoutRequestDto
{
    public Guid Id { get; set; }
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string TutorEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
    public BankAccountDetailsDto BankAccount { get; set; } = new();
    public List<EarningSummaryDto> EarningsHistory { get; set; } = new();
}

public class BankAccountDetailsDto
{
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IfscCode { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string AccountType { get; set; } = string.Empty;
}

public class EarningSummaryDto
{
    public decimal Amount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ApprovePayoutRequest
{
    public string? Notes { get; set; }
}

public class RejectPayoutRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class MarkPaidRequest
{
    public string? TransactionReference { get; set; }
}
