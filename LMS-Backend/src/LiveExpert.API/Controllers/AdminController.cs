using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AdminController : BaseController
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<WithdrawalRequest> _withdrawalRepository;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminController(
        IMediator mediator,
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<Session> sessionRepository,
        IRepository<Payment> paymentRepository,
        IRepository<WithdrawalRequest> withdrawalRepository,
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork) : base(mediator)
    {
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
        _sessionRepository = sessionRepository;
        _paymentRepository = paymentRepository;
        _withdrawalRepository = withdrawalRepository;
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Seed initial subjects (one-time setup)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("seed-subjects")]
    public async Task<IActionResult> SeedSubjects()
    {
        var subjects = new List<Subject>
        {
            new Subject { Name = "Frontend Development", Description = "React, Vue, HTML, CSS", IsActive = true },
            new Subject { Name = "Backend Development", Description = "Node.js, C#, Python backend", IsActive = true },
            new Subject { Name = "Mobile App Development", Description = "React Native, Swift", IsActive = true },
            new Subject { Name = "Python Programming", Description = "Python basics", IsActive = true },
            new Subject { Name = "Java Programming", Description = "Core Java", IsActive = true },
            new Subject { Name = "C++ / C# Programming", Description = "System programming", IsActive = true },
            new Subject { Name = "Machine Learning", Description = "ML algorithms", IsActive = true },
            new Subject { Name = "Artificial Intelligence", Description = "AI models", IsActive = true },
            new Subject { Name = "Cloud Computing", Description = "AWS, Azure", IsActive = true },
            new Subject { Name = "Statistics", Description = "Data analysis", IsActive = true },
            new Subject { Name = "Physics", Description = "Classical physics", IsActive = true },
            new Subject { Name = "Chemistry", Description = "General chemistry", IsActive = true },
            new Subject { Name = "Biology", Description = "Life sciences", IsActive = true },
            new Subject { Name = "History", Description = "World history", IsActive = true },
            new Subject { Name = "Geography", Description = "Physical geography", IsActive = true },
            new Subject { Name = "Economics", Description = "Macro economics", IsActive = true },
            new Subject { Name = "Accounting", Description = "Financial accounting", IsActive = true }
        };

        _context.Subjects.AddRange(subjects);
        await _context.SaveChangesAsync(CancellationToken.None);
        return Ok("Seeded");
    }

    /// <summary>
    /// Get pending tutor verifications (Admin only)
    /// </summary>
    [HttpGet("tutors/pending")]
    public async Task<IActionResult> GetPendingTutors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var allPendingTutors = await _tutorRepository.FindAsync(t => t.VerificationStatus == VerificationStatus.Pending);

        var tutors = allPendingTutors
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(Result<object>.SuccessResult(new
        {
            Items = tutors,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = allPendingTutors.Count()
            }
        }));
    }

    /// <summary>
    /// Approve tutor verification (Admin only)
    /// </summary>
    [HttpPut("tutors/{tutorId}/approve")]
    public async Task<IActionResult> ApproveTutor(Guid tutorId)
    {
        var tutor = await _tutorRepository.GetByIdAsync(tutorId);
        if (tutor == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Tutor not found"));
        }

        tutor.VerificationStatus = VerificationStatus.Approved;
        tutor.UpdatedAt = DateTime.UtcNow;

        await _tutorRepository.UpdateAsync(tutor);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("Tutor approved successfully"));
    }

    /// <summary>
    /// Reject tutor verification (Admin only)
    /// </summary>
    [HttpPut("tutors/{tutorId}/reject")]
    public async Task<IActionResult> RejectTutor(Guid tutorId, [FromBody] RejectTutorRequest request)
    {
        var tutor = await _tutorRepository.GetByIdAsync(tutorId);
        if (tutor == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Tutor not found"));
        }

        tutor.VerificationStatus = VerificationStatus.Rejected;
        tutor.UpdatedAt = DateTime.UtcNow;

        await _tutorRepository.UpdateAsync(tutor);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("Tutor rejected"));
    }

    /// <summary>
    /// Get platform statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var allUsers = await _userRepository.GetAllAsync();
        var allSessions = await _sessionRepository.GetAllAsync();
        var allPayments = await _paymentRepository.GetAllAsync();

        var stats = new
        {
            TotalUsers = allUsers.Count(),
            ActiveUsers = allUsers.Count(u => u.IsActive),
            TotalStudents = allUsers.Count(u => u.Role == UserRole.Student),
            TotalTutors = allUsers.Count(u => u.Role == UserRole.Tutor),
            TotalAdmins = allUsers.Count(u => u.Role == UserRole.Admin),
            TotalSessions = allSessions.Count(),
            CompletedSessions = allSessions.Count(s => s.Status == SessionStatus.Completed),
            TotalRevenue = allPayments.Where(p => p.Status == PaymentStatus.Success).Sum(p => p.TotalAmount),
            TotalTransactions = allPayments.Count(),
            NewUsersToday = allUsers.Count(u => u.CreatedAt.Date == DateTime.UtcNow.Date),
            NewUsersThisWeek = allUsers.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
            NewUsersThisMonth = allUsers.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30))
        };

        return Ok(Result<object>.SuccessResult(stats));
    }

    /// <summary>
    /// Get all sessions (Admin only)
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetAllSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var allSessions = await _sessionRepository.GetAllAsync();

        var sessions = allSessions
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(Result<object>.SuccessResult(new
        {
            Items = sessions,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = allSessions.Count()
            }
        }));
    }

    /// <summary>
    /// Get all payments (Admin only)
    /// </summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetAllPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var allPayments = await _paymentRepository.GetAllAsync();

        var payments = allPayments
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(Result<object>.SuccessResult(new
        {
            Items = payments,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = allPayments.Count()
            }
        }));
    }

    /// <summary>
    /// Get all withdrawal requests (Admin only)
    /// </summary>
    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetAllWithdrawals([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] WithdrawalStatus? status = null)
    {
        var allWithdrawals = await _withdrawalRepository.GetAllAsync();

        if (status.HasValue)
        {
            allWithdrawals = allWithdrawals.Where(w => w.Status == status.Value);
        }

        var withdrawals = allWithdrawals
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new
            {
                Id = w.Id.ToString(),
                Amount = w.Amount,
                Status = w.Status.ToString(),
                RequestedAt = w.CreatedAt,
                ProcessedAt = w.ProcessedAt,
                TutorId = w.UserId.ToString(),
                TutorName = w.User != null ? w.User.Username : "Unknown",
                BankAccountName = w.BankAccount != null ? w.BankAccount.AccountHolderName : null,
                BankAccountNumber = w.BankAccount != null ? "****" + w.BankAccount.AccountNumber.Substring(Math.Max(0, w.BankAccount.AccountNumber.Length - 4)) : null,
                Method = w.BankAccount != null ? "Bank Transfer" : "Unknown"
            })
            .ToList();

        return Ok(Result<object>.SuccessResult(new
        {
            Items = withdrawals,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = allWithdrawals.Count()
            }
        }));
    }

    /// <summary>
    /// Approve a withdrawal request (Admin only)
    /// </summary>
    [HttpPut("withdrawals/{withdrawalId}/approve")]
    public async Task<IActionResult> ApproveWithdrawal(Guid withdrawalId)
    {
        var withdrawal = await _withdrawalRepository.GetByIdAsync(withdrawalId);
        if (withdrawal == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Withdrawal request not found"));
        }

        if (withdrawal.Status != WithdrawalStatus.Pending)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Only pending withdrawals can be approved"));
        }

        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        withdrawal.Status = WithdrawalStatus.Approved;
        withdrawal.ProcessedBy = currentUserId.Value;
        withdrawal.ProcessedAt = DateTime.UtcNow;

        await _withdrawalRepository.UpdateAsync(withdrawal);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("Withdrawal approved successfully"));
    }

    /// <summary>
    /// Reject a withdrawal request (Admin only)
    /// </summary>
    [HttpPut("withdrawals/{withdrawalId}/reject")]
    public async Task<IActionResult> RejectWithdrawal(Guid withdrawalId, [FromBody] RejectWithdrawalRequest? request = null)
    {
        var withdrawal = await _withdrawalRepository.GetByIdAsync(withdrawalId);
        if (withdrawal == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Withdrawal request not found"));
        }

        if (withdrawal.Status != WithdrawalStatus.Pending)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Only pending withdrawals can be rejected"));
        }

        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        withdrawal.Status = WithdrawalStatus.Rejected;
        withdrawal.ProcessedBy = currentUserId.Value;
        withdrawal.ProcessedAt = DateTime.UtcNow;
        withdrawal.RejectionReason = request?.Reason;

        await _withdrawalRepository.UpdateAsync(withdrawal);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("Withdrawal rejected successfully"));
    }
}

public class RejectTutorRequest
{
    public string? Reason { get; set; }
}

public class RejectWithdrawalRequest
{
    public string? Reason { get; set; }
}
