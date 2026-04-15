using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Withdrawals.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Withdrawals.Handlers;

// Request Withdrawal Handler
public class RequestWithdrawalCommandHandler : IRequestHandler<RequestWithdrawalCommand, Result<Guid>>
{
    private readonly IRepository<WithdrawalRequest> _withdrawalRepository;
    private readonly IRepository<TutorEarning> _tutorEarningRepository;
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISystemSettingsService _settingsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public RequestWithdrawalCommandHandler(
        IRepository<WithdrawalRequest> withdrawalRepository,
        IRepository<TutorEarning> tutorEarningRepository,
        IRepository<BankAccount> bankAccountRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        ISystemSettingsService settingsService,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _withdrawalRepository = withdrawalRepository;
        _tutorEarningRepository = tutorEarningRepository;
        _bankAccountRepository = bankAccountRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _settingsService = settingsService;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(RequestWithdrawalCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<Guid>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null || user.Role != UserRole.Tutor)
            return Result<Guid>.FailureResult("FORBIDDEN", "Only tutors can request withdrawals");

        // Check minimum withdrawal amount
        var minAmount = await _settingsService.GetMinWithdrawalAmountAsync();
        if (request.Amount < minAmount)
            return Result<Guid>.FailureResult("INVALID_AMOUNT", $"Minimum withdrawal amount is ₹{minAmount}");

        // Verify bank account
        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bankAccount == null || bankAccount.UserId != userId.Value)
            return Result<Guid>.FailureResult("INVALID_BANK_ACCOUNT", "Invalid bank account");

        var earnings = await _tutorEarningRepository.FindAsync(
            e => e.TutorId == userId.Value && e.Status == EarningStatus.Available,
            cancellationToken);

        var availableForWithdrawal = earnings.Sum(e => e.NetAmount);
        if (availableForWithdrawal < request.Amount)
            return Result<Guid>.FailureResult("INSUFFICIENT_BALANCE", "Insufficient earnings balance");

        // Enforce MaxPayoutPercentage — tutor must retain at least (100 - maxPct)% of their balance
        var maxPayoutPct = await _settingsService.GetMaxPayoutPercentageAsync();
        var maxAllowed = Math.Round(availableForWithdrawal * (maxPayoutPct / 100m), 2);
        if (request.Amount > maxAllowed)
            return Result<Guid>.FailureResult("EXCEEDS_MAX_PAYOUT",
                $"You can withdraw at most ₹{maxAllowed} ({maxPayoutPct}% of your available balance of ₹{availableForWithdrawal}).");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create withdrawal request
            var withdrawal = new WithdrawalRequest
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                Amount = request.Amount,
                BankAccountId = request.BankAccountId,
                Status = WithdrawalStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _withdrawalRepository.AddAsync(withdrawal, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Send notification
            await _notificationService.SendNotificationAsync(
                userId.Value,
                "Withdrawal Request Submitted",
                $"Your withdrawal request for ₹{request.Amount} has been submitted and is pending approval.",
                null,
                null
            );

            return Result<Guid>.SuccessResult(withdrawal.Id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

// Get Withdrawals Handler
public class GetWithdrawalsQueryHandler : IRequestHandler<GetWithdrawalsQuery, Result<PaginatedResult<WithdrawalDto>>>
{
    private readonly IRepository<WithdrawalRequest> _withdrawalRepository;
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetWithdrawalsQueryHandler(
        IRepository<WithdrawalRequest> withdrawalRepository,
        IRepository<BankAccount> bankAccountRepository,
        ICurrentUserService currentUserService)
    {
        _withdrawalRepository = withdrawalRepository;
        _bankAccountRepository = bankAccountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<WithdrawalDto>>> Handle(GetWithdrawalsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<PaginatedResult<WithdrawalDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var query = await _withdrawalRepository.FindAsync(w => w.UserId == userId.Value, cancellationToken);

        if (request.Status.HasValue)
            query = query.Where(w => w.Status == request.Status.Value);

        var allWithdrawals = query.OrderByDescending(w => w.CreatedAt).ToList();

        var withdrawals = allWithdrawals
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = new List<WithdrawalDto>();
        foreach (var withdrawal in withdrawals)
        {
            var bankAccount = withdrawal.BankAccountId.HasValue 
                ? await _bankAccountRepository.GetByIdAsync(withdrawal.BankAccountId.Value, cancellationToken)
                : null;
            
            dtos.Add(new WithdrawalDto
            {
                Id = withdrawal.Id,
                Amount = withdrawal.Amount,
                Status = withdrawal.Status.ToString(),
                RequestedAt = withdrawal.CreatedAt,
                ProcessedAt = withdrawal.ProcessedAt,
                BankAccountName = bankAccount?.AccountHolderName,
                BankAccountNumber = bankAccount != null ? "****" + bankAccount.AccountNumber.Substring(Math.Max(0, bankAccount.AccountNumber.Length - 4)) : null
            });
        }

        var result = new PaginatedResult<WithdrawalDto>
        {
            Items = dtos,
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalRecords = allWithdrawals.Count,
                TotalPages = (int)Math.Ceiling(allWithdrawals.Count / (double)request.PageSize)
            }
        };

        return Result<PaginatedResult<WithdrawalDto>>.SuccessResult(result);
    }
}

// Add Bank Account Handler
public class AddBankAccountCommandHandler : IRequestHandler<AddBankAccountCommand, Result<Guid>>
{
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddBankAccountCommandHandler(
        IRepository<BankAccount> bankAccountRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bankAccountRepository = bankAccountRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddBankAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<Guid>.FailureResult("UNAUTHORIZED", "User not authenticated");

        // Check if account already exists
        var existing = await _bankAccountRepository.FirstOrDefaultAsync(
            b => b.UserId == userId.Value && b.AccountNumber == request.AccountNumber,
            cancellationToken);

        if (existing != null)
            return Result<Guid>.FailureResult("DUPLICATE", "Bank account already exists");

        // Parse account type enum
        var accountType = request.AccountType.ToLower() == "current" 
            ? AccountType.Current 
            : AccountType.Savings;

        var bankAccount = new BankAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            AccountHolderName = request.AccountHolderName,
            AccountNumber = request.AccountNumber,
            BankName = request.BankName,
            IFSCCode = request.IFSCCode,
            AccountType = accountType,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _bankAccountRepository.AddAsync(bankAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.SuccessResult(bankAccount.Id);
    }
}

// Get Bank Accounts Handler
public class GetBankAccountsQueryHandler : IRequestHandler<GetBankAccountsQuery, Result<List<BankAccountDto>>>
{
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetBankAccountsQueryHandler(
        IRepository<BankAccount> bankAccountRepository,
        ICurrentUserService currentUserService)
    {
        _bankAccountRepository = bankAccountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<BankAccountDto>>> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<BankAccountDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var accounts = await _bankAccountRepository.FindAsync(
            b => b.UserId == userId.Value, cancellationToken);

        var dtos = accounts.Select(a => new BankAccountDto
        {
            Id = a.Id,
            AccountHolderName = a.AccountHolderName,
            AccountNumber = "****" + a.AccountNumber.Substring(Math.Max(0, a.AccountNumber.Length - 4)),
            BankName = a.BankName,
            IFSCCode = a.IFSCCode,
            IsDefault = false, // Will be added in Option B
            IsVerified = a.IsVerified
        }).ToList();

        return Result<List<BankAccountDto>>.SuccessResult(dtos);
    }
}

// Delete Bank Account Handler
public class DeleteBankAccountCommandHandler : IRequestHandler<DeleteBankAccountCommand, Result>
{
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBankAccountCommandHandler(
        IRepository<BankAccount> bankAccountRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _bankAccountRepository = bankAccountRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");

        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bankAccount == null || bankAccount.UserId != userId.Value)
            return Result.FailureResult("NOT_FOUND", "Bank account not found");

        await _bankAccountRepository.DeleteAsync(bankAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Bank account deleted successfully");
    }
}
