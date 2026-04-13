using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Tutor bank account management
/// </summary>
[Authorize(Roles = "Tutor")]
[Route("api/tutor/bank-accounts")]
[ApiController]
public class TutorBankAccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;

    public TutorBankAccountsController(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Get all bank accounts for current tutor
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<BankAccountDto>>), 200)]
    public async Task<IActionResult> GetBankAccounts()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<List<BankAccountDto>>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var accounts = await _context.BankAccounts
            .Where(b => b.UserId == userId.Value)
            .OrderByDescending(b => b.IsPrimary)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();

        var accountDtos = accounts.Select(a => new BankAccountDto
        {
            Id = a.Id,
            AccountHolderName = a.AccountHolderName,
            AccountNumber = MaskAccountNumber(_encryptionService.Decrypt(a.AccountNumber)),
            BankName = a.BankName,
            IFSCCode = a.IFSCCode,
            BranchName = a.BranchName,
            AccountType = a.AccountType.ToString(),
            IsPrimary = a.IsPrimary,
            IsVerified = a.IsVerified,
        }).ToList();

        return Ok(Result<List<BankAccountDto>>.SuccessResult(accountDtos));
    }

    /// <summary>
    /// Add a new bank account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<BankAccountDto>), 201)]
    public async Task<IActionResult> AddBankAccount([FromBody] AddBankAccountRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<BankAccountDto>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        // If this is set as primary, unset other primary accounts
        if (request.IsPrimary)
        {
            var existingPrimary = await _context.BankAccounts
                .Where(b => b.UserId == userId.Value && b.IsPrimary)
                .ToListAsync();
            
            foreach (var existingAccount in existingPrimary)
            {
                existingAccount.IsPrimary = false;
            }
        }

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            AccountHolderName = request.AccountHolderName,
            AccountNumber = _encryptionService.Encrypt(request.AccountNumber),
            BankName = request.BankName,
            IFSCCode = request.IFSCCode,
            BranchName = request.BranchName,
            AccountType = Enum.Parse<AccountType>(request.AccountType),
            IsPrimary = request.IsPrimary,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        var accountDto = new BankAccountDto
        {
            Id = account.Id,
            AccountHolderName = account.AccountHolderName,
            AccountNumber = MaskAccountNumber(request.AccountNumber),
            BankName = account.BankName,
            IFSCCode = account.IFSCCode,
            BranchName = account.BranchName,
            AccountType = account.AccountType.ToString(),
            IsPrimary = account.IsPrimary,
            IsVerified = account.IsVerified,
        };

        return CreatedAtAction(nameof(GetBankAccounts), new { id = account.Id }, 
            Result<BankAccountDto>.SuccessResult(accountDto));
    }

    /// <summary>
    /// Update bank account
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<BankAccountDto>), 200)]
    public async Task<IActionResult> UpdateBankAccount(Guid id, [FromBody] UpdateBankAccountRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<BankAccountDto>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId.Value);

        if (account == null)
        {
            return NotFound(Result<BankAccountDto>.FailureResult("NOT_FOUND", "Bank account not found"));
        }

        // If setting as primary, unset other primary accounts
        if (request.IsPrimary && !account.IsPrimary)
        {
            var existingPrimary = await _context.BankAccounts
                .Where(b => b.UserId == userId.Value && b.IsPrimary && b.Id != id)
                .ToListAsync();
            
            foreach (var acc in existingPrimary)
            {
                acc.IsPrimary = false;
            }
        }

        account.AccountHolderName = request.AccountHolderName;
        if (!string.IsNullOrEmpty(request.AccountNumber))
        {
            account.AccountNumber = _encryptionService.Encrypt(request.AccountNumber);
        }
        account.BankName = request.BankName;
        account.IFSCCode = request.IFSCCode;
        account.BranchName = request.BranchName;
        account.AccountType = Enum.Parse<AccountType>(request.AccountType);
        account.IsPrimary = request.IsPrimary;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var accountDto = new BankAccountDto
        {
            Id = account.Id,
            AccountHolderName = account.AccountHolderName,
            AccountNumber = MaskAccountNumber(_encryptionService.Decrypt(account.AccountNumber)),
            BankName = account.BankName,
            IFSCCode = account.IFSCCode,
            BranchName = account.BranchName,
            AccountType = account.AccountType.ToString(),
            IsPrimary = account.IsPrimary,
            IsVerified = account.IsVerified,
        };

        return Ok(Result<BankAccountDto>.SuccessResult(accountDto));
    }

    /// <summary>
    /// Delete bank account
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> DeleteBankAccount(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId.Value);

        if (account == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Bank account not found"));
        }

        // Check if account is used in pending payout requests
        var hasPendingPayouts = await _context.PayoutRequests
            .AnyAsync(p => p.BankAccountId == id && 
                          p.Status == PayoutStatus.Pending);

        if (hasPendingPayouts)
        {
            return BadRequest(Result.FailureResult("IN_USE", 
                "Cannot delete bank account with pending payout requests"));
        }

        _context.BankAccounts.Remove(account);
        await _context.SaveChangesAsync();

        return Ok(Result.SuccessResult());
    }

    private string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return "****";
        
        return "****" + accountNumber.Substring(accountNumber.Length - 4);
    }
}

public class BankAccountDto
{
    public Guid Id { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty; // Masked
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
}

public class AddBankAccountRequest
{
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string AccountType { get; set; } = "Savings";
    public bool IsPrimary { get; set; }
}

public class UpdateBankAccountRequest
{
    public string AccountHolderName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string AccountType { get; set; } = "Savings";
    public bool IsPrimary { get; set; }
}
