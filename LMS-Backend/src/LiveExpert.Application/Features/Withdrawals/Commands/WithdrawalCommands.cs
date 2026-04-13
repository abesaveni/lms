using LiveExpert.Application.Common;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Withdrawals.Commands;

// Request Withdrawal Command
public class RequestWithdrawalCommand : IRequest<Result<Guid>>
{
    public decimal Amount { get; set; }
    public Guid BankAccountId { get; set; }
}

// Get Withdrawals Query
public class GetWithdrawalsQuery : IRequest<Result<PaginatedResult<WithdrawalDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public WithdrawalStatus? Status { get; set; }
}

public class WithdrawalDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
}

// Add Bank Account Command
public class AddBankAccountCommand : IRequest<Result<Guid>>
{
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Savings";
}

// Get Bank Accounts Query
public class GetBankAccountsQuery : IRequest<Result<List<BankAccountDto>>>
{
    // Uses current user
}

public class BankAccountDto
{
    public Guid Id { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }
}

// Delete Bank Account Command
public class DeleteBankAccountCommand : IRequest<Result>
{
    public Guid BankAccountId { get; set; }
}
