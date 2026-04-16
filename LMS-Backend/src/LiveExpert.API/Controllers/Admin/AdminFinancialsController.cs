using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin financials overview
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin/financials")]
[ApiController]
public class AdminFinancialsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminFinancialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get financial overview with paginated transaction history
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFinancials(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Summary aggregates from Payments table (source of truth for money in)
        var completedPayments = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Success)
            .ToListAsync();

        var totalRevenue       = completedPayments.Sum(p => p.TotalAmount);
        var totalPlatformFees  = completedPayments.Sum(p => p.PlatformFee);
        var totalTutorEarnings = totalRevenue - totalPlatformFees;

        var totalSessionBookings = await _context.SessionBookings.CountAsync();

        var totalWithdrawals = await _context.WithdrawalRequests
            .Where(w => w.Status == WithdrawalStatus.Completed)
            .SumAsync(w => (decimal?)w.Amount) ?? 0m;

        // net profit = platform fees collected minus payouts made
        var totalPaidOut = await _context.PayoutRequests
            .Where(p => p.Status == PayoutStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var netProfit = totalPlatformFees - totalPaidOut;

        // Paginated transaction list from TutorEarnings
        var earningsQuery = _context.TutorEarnings
            .OrderByDescending(e => e.CreatedAt);

        var totalRecords = await earningsQuery.CountAsync();

        var earnings = await earningsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var transactions = earnings.Select(e => new
        {
            id        = e.Id.ToString(),
            type      = e.SourceType ?? "Earning",
            userId    = e.TutorId.ToString(),
            sessionId = e.SourceType == "Session" ? e.SourceId.ToString() : (string?)null,
            amount    = e.Amount,
            status    = e.Status.ToString(),
            createdAt = e.CreatedAt
        }).ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                summary = new
                {
                    totalRevenue,
                    totalSessionBookings,
                    totalWithdrawals,
                    totalPlatformFees,
                    totalTutorEarnings,
                    netProfit
                },
                transactions,
                pagination = new
                {
                    currentPage  = page,
                    pageSize,
                    totalRecords
                }
            }
        });
    }
}
