using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Tutor payout requests
/// </summary>
[Authorize(Roles = "Tutor")]
[Route("api/tutor/payouts")]
[ApiController]
public class TutorPayoutsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _config;

    public TutorPayoutsController(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        INotificationService notificationService,
        IEncryptionService encryptionService,
        IConfiguration config)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _encryptionService = encryptionService;
        _config = config;
    }

    /// <summary>
    /// Wallet summary — available balance, max payout (90%), reserve (10%)
    /// </summary>
    [HttpGet("wallet")]
    public async Task<IActionResult> GetWallet()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return Unauthorized();

        // NetAmount is a C# computed property (Amount - CommissionAmount) — use inline arithmetic
        // so EF Core can translate it to SQL
        var available = await _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value && e.Status == EarningStatus.Available)
            .SumAsync(e => (decimal?)(e.Amount - e.CommissionAmount)) ?? 0m;

        var pending = await _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value && e.Status == EarningStatus.Pending)
            .SumAsync(e => (decimal?)(e.Amount - e.CommissionAmount)) ?? 0m;

        var paid = await _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value && e.Status == EarningStatus.Paid)
            .SumAsync(e => (decimal?)(e.Amount - e.CommissionAmount)) ?? 0m;

        var maxPayoutPct = _config.GetValue<decimal>("AppSettings:MaxPayoutPercentage", 90m);
        var maxPayout = Math.Round(available * maxPayoutPct / 100m, 2);
        var reserve = available - maxPayout;

        // Next release: earliest pending earning's AvailableAt
        var nextRelease = await _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value && e.Status == EarningStatus.Pending && e.AvailableAt.HasValue)
            .OrderBy(e => e.AvailableAt)
            .Select(e => e.AvailableAt)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            available,
            pending,
            totalPaid = paid,
            maxPayout,
            reserve,
            maxPayoutPercent = maxPayoutPct,
            nextReleaseAt = nextRelease,
            hasBankAccount = await _context.BankAccounts.AnyAsync(b => b.UserId == userId.Value)
        });
    }

    /// <summary>
    /// Request a payout
    /// </summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(Result<PayoutRequestDto>), 201)]
    public async Task<IActionResult> RequestPayout([FromBody] RequestPayoutRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<PayoutRequestDto>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        // Check if tutor has any bank accounts
        var hasBankAccounts = await _context.BankAccounts
            .AnyAsync(b => b.UserId == userId.Value);

        if (!hasBankAccounts)
        {
            return BadRequest(Result<PayoutRequestDto>.FailureResult("NO_BANK_ACCOUNT", 
                "You must add at least one bank account in your settings before requesting a payout."));
        }

        // Check if bank account exists and belongs to tutor
        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == request.BankAccountId && b.UserId == userId.Value);

        if (bankAccount == null)
        {
            return NotFound(Result<PayoutRequestDto>.FailureResult("NOT_FOUND", "Bank account not found"));
        }

        // Check available earnings (NetAmount is computed — use inline arithmetic for EF Core SQL translation)
        var availableEarnings = await _context.TutorEarnings
            .Where(e => e.TutorId == userId.Value && e.Status == EarningStatus.Available)
            .SumAsync(e => e.Amount - e.CommissionAmount);

        if (availableEarnings <= 0)
        {
            return BadRequest(Result<PayoutRequestDto>.FailureResult("NO_FUNDS",
                "You have no available earnings to withdraw."));
        }

        // 90% rule: tutor can request up to 90% of available balance; 10% stays as reserve
        var maxPayoutPct = _config.GetValue<decimal>("AppSettings:MaxPayoutPercentage", 90m);
        var maxAllowed = Math.Round(availableEarnings * maxPayoutPct / 100m, 2);

        if (request.Amount <= 0)
        {
            return BadRequest(Result<PayoutRequestDto>.FailureResult("INVALID_AMOUNT",
                "Amount must be greater than zero."));
        }

        if (request.Amount > maxAllowed)
        {
            return BadRequest(Result<PayoutRequestDto>.FailureResult("EXCEEDS_LIMIT",
                $"Maximum payout is {maxPayoutPct}% of available balance: ₹{maxAllowed:F2}. " +
                $"A 10% reserve (₹{availableEarnings - maxAllowed:F2}) remains in your wallet."));
        }

        // Create payout request
        var payoutRequest = new PayoutRequest
        {
            Id = Guid.NewGuid(),
            TutorId = userId.Value,
            BankAccountId = request.BankAccountId,
            Amount = request.Amount,
            Status = PayoutStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            PaymentMethod = "Bank Transfer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.PayoutRequests.Add(payoutRequest);
        await _context.SaveChangesAsync();

        // Get tutor details for notification
        var tutor = await _context.Users
            .Include(u => u.TutorProfile)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        // Send notification to all admins
        var admins = await _userRepository.FindAsync(u => u.Role == UserRole.Admin);
        var tutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "A tutor";
        if (string.IsNullOrWhiteSpace(tutorName)) tutorName = tutor?.Username ?? "A tutor";

        var notificationMessage = $"New payout request from {tutorName}:\n" +
            $"Amount: ₹{request.Amount:N2}\n" +
            $"Bank: {bankAccount.BankName}\n" +
            $"Account: {bankAccount.AccountHolderName}\n" +
            $"IFSC: {bankAccount.IFSCCode}\n" +
            $"Account Number: {_encryptionService.Decrypt(bankAccount.AccountNumber)}\n" +
            $"Request ID: {payoutRequest.Id}";

        foreach (var admin in admins)
        {
            await _notificationService.SendNotificationAsync(
                admin.Id,
                "New Payout Request",
                notificationMessage,
                NotificationType.PayoutRequest,
                $"/admin/payouts?status=Pending",
                default);
        }

        var payoutDto = new PayoutRequestDto
        {
            Id = payoutRequest.Id,
            Amount = payoutRequest.Amount,
            Status = payoutRequest.Status.ToString(),
            RequestedAt = payoutRequest.RequestedAt,
            ProcessedAt = payoutRequest.ProcessedAt,
            AdminNotes = payoutRequest.AdminNotes,
            BankAccount = new BankAccountSummaryDto
            {
                AccountHolderName = bankAccount.AccountHolderName,
                BankName = bankAccount.BankName,
                AccountNumber = "****" + "XXXX", // Masked
            },
        };

        return CreatedAtAction(nameof(GetPayoutHistory), new { id = payoutRequest.Id }, 
            Result<PayoutRequestDto>.SuccessResult(payoutDto));
    }

    /// <summary>
    /// Get payout history
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<PayoutRequestDto>>), 200)]
    public async Task<IActionResult> GetPayoutHistory([FromQuery] string? status)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<List<PayoutRequestDto>>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var query = _context.PayoutRequests
            .Include(p => p.BankAccount)
            .Where(p => p.TutorId == userId.Value)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<PayoutStatus>(status, out var payoutStatus))
            {
                query = query.Where(p => p.Status == payoutStatus);
            }
        }

        var payouts = await query
            .OrderByDescending(p => p.RequestedAt)
            .ToListAsync();

        var payoutDtos = payouts.Select(p => new PayoutRequestDto
        {
            Id = p.Id,
            Amount = p.Amount,
            Status = p.Status.ToString(),
            RequestedAt = p.RequestedAt,
            ProcessedAt = p.ProcessedAt,
            AdminNotes = p.AdminNotes,
            BankAccount = p.BankAccount != null ? new BankAccountSummaryDto
            {
                AccountHolderName = p.BankAccount.AccountHolderName,
                BankName = p.BankAccount.BankName,
                AccountNumber = "****XXXX", // Masked
            } : null,
        }).ToList();

        return Ok(Result<List<PayoutRequestDto>>.SuccessResult(payoutDtos));
    }
}

public class RequestPayoutRequest
{
    public Guid BankAccountId { get; set; }
    public decimal Amount { get; set; }
}

public class PayoutRequestDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? AdminNotes { get; set; }
    public BankAccountSummaryDto BankAccount { get; set; } = new();
}

public class BankAccountSummaryDto
{
    public string AccountHolderName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}
