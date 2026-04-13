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
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly IRepository<StudentProfile> _studentRepository;
    private readonly IRepository<SessionBooking> _sessionBookingRepository;
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly IRepository<BonusPoint> _bonusPointRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<UserConsent> _userConsentRepository;
    private readonly IRepository<UserCalendarConnection> _calendarConnectionRepository;
    private readonly IRepository<TutorFollower> _tutorFollowerRepository;
    private readonly IRepository<PayoutRequest> _payoutRequestRepository;
    private readonly IRepository<TutorEarning> _tutorEarningRepository;
    private readonly IRepository<Referral> _referralTrackRepository;
    private readonly IRepository<KYCDocument> _kycDocumentRepository;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IMediator mediator,
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<Session> sessionRepository,
        IRepository<Payment> paymentRepository,
        IRepository<WithdrawalRequest> withdrawalRepository,
        IRepository<BankAccount> bankAccountRepository,
        IRepository<StudentProfile> studentRepository,
        IRepository<SessionBooking> sessionBookingRepository,
        IRepository<ReferralProgram> referralRepository,
        IRepository<BonusPoint> bonusPointRepository,
        IRepository<Message> messageRepository,
        IRepository<Review> reviewRepository,
        IRepository<Notification> notificationRepository,
        IRepository<UserConsent> userConsentRepository,
        IRepository<UserCalendarConnection> calendarConnectionRepository,
        IRepository<TutorFollower> tutorFollowerRepository,
        IRepository<PayoutRequest> payoutRequestRepository,
        IRepository<TutorEarning> tutorEarningRepository,
        IRepository<Referral> referralTrackRepository,
        IRepository<KYCDocument> kycDocumentRepository,
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        ILogger<AdminController> logger) : base(mediator)
    {
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
        _sessionRepository = sessionRepository;
        _paymentRepository = paymentRepository;
        _withdrawalRepository = withdrawalRepository;
        _bankAccountRepository = bankAccountRepository;
        _studentRepository = studentRepository;
        _sessionBookingRepository = sessionBookingRepository;
        _referralRepository = referralRepository;
        _bonusPointRepository = bonusPointRepository;
        _messageRepository = messageRepository;
        _reviewRepository = reviewRepository;
        _notificationRepository = notificationRepository;
        _userConsentRepository = userConsentRepository;
        _calendarConnectionRepository = calendarConnectionRepository;
        _tutorFollowerRepository = tutorFollowerRepository;
        _payoutRequestRepository = payoutRequestRepository;
        _tutorEarningRepository = tutorEarningRepository;
        _referralTrackRepository = referralTrackRepository;
        _kycDocumentRepository = kycDocumentRepository;
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only)
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

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] UserRole? role = null)
    {
        var allUsersQuery = _userRepository.GetQueryable()
            .Include(u => u.TutorProfile)
            .AsQueryable();

        if (role.HasValue)
        {
            allUsersQuery = allUsersQuery.Where(u => u.Role == role.Value);
        }

        var allUsersList = await allUsersQuery.ToListAsync();
        var users = allUsersList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.PhoneNumber,
                Role = u.Role.ToString(),
                u.IsActive,
                u.IsEmailVerified,
                u.IsPhoneVerified,
                u.CreatedAt,
                u.LastLoginAt,
                VerificationStatus = u.Role == UserRole.Tutor && u.TutorProfile != null 
                    ? u.TutorProfile.VerificationStatus.ToString() 
                    : null
            })
            .ToList();

        return Ok(Result<object>.SuccessResult(new
        {
            Items = users,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = allUsersList.Count()
            }
        }));
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(Result.FailureResult("VALIDATION_ERROR", "Username and email are required"));
            }

            // Check if username already exists
            if (await _userRepository.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(Result.FailureResult("VALIDATION_ERROR", "Username already exists"));
            }

            // Check if email already exists
            if (await _userRepository.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(Result.FailureResult("VALIDATION_ERROR", "Email already exists"));
            }

            // Parse role
            if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            {
                return BadRequest(Result.FailureResult("VALIDATION_ERROR", "Invalid role"));
            }

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                WhatsAppNumber = request.PhoneNumber ?? string.Empty,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                PasswordHash = _encryptionService.Hash(request.Password ?? "Default@123"),
                Role = userRole,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                IsWhatsAppVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Create profile based on role
            if (userRole == UserRole.Tutor)
            {
                var tutorProfile = new TutorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    VerificationStatus = VerificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _tutorRepository.AddAsync(tutorProfile);
            }
            else if (userRole == UserRole.Student)
            {
                var studentProfile = new StudentProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ReferralCode = $"{user.Username.ToUpper().Substring(0, Math.Min(3, user.Username.Length))}{new Random().Next(1000, 9999)}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _studentRepository.AddAsync(studentProfile);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(Result<object>.SuccessResult(new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Result.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}"));
        }
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
    /// Deactivate user (Admin only)
    /// </summary>
    [HttpPut("users/{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));
        }

        // Prevent super admin deactivation
        if (user.Email == "superadmin@liveexpert.ai")
        {
            return BadRequest(Result.FailureResult("FORBIDDEN", "Super admin cannot be deactivated"));
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("User deactivated"));
    }

    /// <summary>
    /// Activate user (Admin only)
    /// </summary>
    [HttpPut("users/{userId}/activate")]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("User activated"));
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetQueryable()
                .Include(u => u.TutorProfile)
                .Include(u => u.StudentProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));
            }

            // Prevent super admin deletion
            if (user.Email == "superadmin@liveexpert.ai")
            {
                return BadRequest(Result.FailureResult("FORBIDDEN", "Super admin cannot be deleted"));
            }

            _logger.LogInformation("Hard deleting user {UserId} and all dependencies using universal context purge", userId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. UNIVERSAL CLEAN UP OF EVERY TABLE (To satisfy SQLite strict FKs)
                
                // Communication & Social
                _context.Messages.RemoveRange(_context.Messages.Where(m => m.SenderId == userId || m.ReceiverId == userId));
                _context.Conversations.RemoveRange(_context.Conversations.Where(c => c.User1Id == userId || c.User2Id == userId));
                _context.Reviews.RemoveRange(_context.Reviews.Where(r => r.StudentId == userId || r.TutorId == userId));
                _context.TutorFollowers.RemoveRange(_context.TutorFollowers.Where(f => f.TutorId == userId || f.StudentId == userId));
                _context.ChatRequests.RemoveRange(_context.ChatRequests.Where(c => c.StudentId == userId || c.TutorId == userId || (c.LastActionById != null && c.LastActionById == userId)));

                // Sessions & Bookings
                var userSessionIds = await _context.Sessions.Where(s => s.TutorId == userId).Select(s => s.Id).ToListAsync();
                _context.SessionBookings.RemoveRange(_context.SessionBookings.Where(b => b.StudentId == userId || userSessionIds.Contains(b.SessionId)));
                _context.SessionMeetLinks.RemoveRange(_context.SessionMeetLinks.Where(l => userSessionIds.Contains(l.SessionId)));
                _context.VirtualClassroomSessions.RemoveRange(_context.VirtualClassroomSessions.Where(v => v.TutorId == userId));
                _context.Sessions.RemoveRange(_context.Sessions.Where(s => s.TutorId == userId));

                // Financials
                _context.Payments.RemoveRange(_context.Payments.Where(p => p.StudentId == userId || p.TutorId == userId));
                _context.WithdrawalRequests.RemoveRange(_context.WithdrawalRequests.Where(w => w.UserId == userId || (w.ProcessedBy != null && w.ProcessedBy == userId)));
                _context.PayoutRequests.RemoveRange(_context.PayoutRequests.Where(p => p.TutorId == userId || (p.ProcessedBy != null && p.ProcessedBy == userId)));
                _context.TutorEarnings.RemoveRange(_context.TutorEarnings.Where(e => e.TutorId == userId));
                _context.BankAccounts.RemoveRange(_context.BankAccounts.Where(b => b.UserId == userId));

                // Tracking & Profile Extras
                _context.Referrals.RemoveRange(_context.Referrals.Where(r => r.ReferrerUserId == userId || r.ReferredUserId == userId));
                _context.KYCDocuments.RemoveRange(_context.KYCDocuments.Where(k => k.UserId == userId || (k.VerifiedBy != null && k.VerifiedBy == userId)));
                _context.Notifications.RemoveRange(_context.Notifications.Where(n => n.UserId == userId));
                _context.UserConsents.RemoveRange(_context.UserConsents.Where(c => c.UserId == userId));
                _context.CookieConsents.RemoveRange(_context.CookieConsents.Where(c => c.UserId == userId));
                _context.BonusPoints.RemoveRange(_context.BonusPoints.Where(b => b.UserId == userId));
                _context.UserNotificationPreferences.RemoveRange(_context.UserNotificationPreferences.Where(p => p.UserId == userId));

                // Admin & System
                _context.AuditLogs.RemoveRange(_context.AuditLogs.Where(a => a.UserId == userId));
                _context.WhatsAppCampaigns.RemoveRange(_context.WhatsAppCampaigns.Where(w => w.CreatedBy == userId));
                _context.AdminPermissions.RemoveRange(_context.AdminPermissions.Where(p => p.AdminId == userId || (p.GrantedBy != null && p.GrantedBy == userId)));
                
                await _context.APIKeys.Where(k => k.UpdatedBy == userId).ExecuteUpdateAsync(s => s.SetProperty(k => k.UpdatedBy, (Guid?)null));
                await _context.SystemSettings.Where(s => s.UpdatedBy == userId).ExecuteUpdateAsync(s => s.SetProperty(s => s.UpdatedBy, (Guid?)null));
                
                _context.TutorGoogleTokens.RemoveRange(_context.TutorGoogleTokens.Where(t => t.TutorId == userId));
                _context.UserCalendarConnections.RemoveRange(_context.UserCalendarConnections.Where(c => c.UserId == userId));
                
                if (user.TutorProfile != null)
                {
                    _context.TutorVerifications.RemoveRange(_context.TutorVerifications.Where(v => v.TutorId == userId));
                    _context.TutorProfiles.Remove(user.TutorProfile);
                }

                if (user.StudentProfile != null)
                {
                    _context.StudentProfiles.Remove(user.StudentProfile);
                }
                
                _context.Users.Remove(user);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully purged user {UserId} from all system tables.", userId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Universal purge failed for user {UserId}", userId);
                throw;
            }

            return Ok(Result.SuccessResult("User and all associated data deleted permanently"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hard delete user {UserId}", userId);
            return StatusCode(500, Result.FailureResult("SERVER_ERROR", $"Deep delete failed. Error: {ex.Message}. Inner: {ex.InnerException?.Message}"));
        }
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
    /// Get comprehensive financial data (Admin only)
    /// </summary>
    [HttpGet("financials")]
    public async Task<IActionResult> GetFinancials([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var allPayments = await _paymentRepository.GetAllAsync();
            var allSessionBookings = await _sessionBookingRepository.GetAllAsync();
            var allWithdrawals = await _withdrawalRepository.GetAllAsync();

            // Calculate totals
            var successfulPayments = allPayments.Where(p => p.Status == PaymentStatus.Success).ToList();
            var totalRevenue = successfulPayments.Sum(p => (double)p.TotalAmount);
            var totalPlatformFees = successfulPayments.Sum(p => (double)p.PlatformFee);
            var totalTutorEarnings = successfulPayments.Sum(p => (double)p.BaseAmount);
            var totalSessionBookings = successfulPayments.Count;

            var totalWithdrawals = allWithdrawals
                .Where(w => w.Status == WithdrawalStatus.Approved || w.Status == WithdrawalStatus.Completed)
                .Sum(w => (double?)w.Amount) ?? 0;

            var paymentTransactions = successfulPayments
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .Select(p => new
                {
                    Id = p.Id.ToString(),
                    Type = "Session Payment",
                    UserId = p.StudentId.ToString(),
                    SessionId = p.SessionId.ToString(),
                    Amount = (double)p.TotalAmount,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt
                })
                .ToList();

            var withdrawalTransactions = allWithdrawals
                .OrderByDescending(w => w.CreatedAt)
                .Take(50)
                .Select(w => new
                {
                    Id = w.Id.ToString(),
                    Type = "Withdrawal",
                    UserId = w.UserId.ToString(),
                    SessionId = (string?)null,
                    Amount = (double)w.Amount,
                    Status = w.Status.ToString(),
                    CreatedAt = w.CreatedAt
                })
                .ToList();

            var allRecentTransactions = paymentTransactions
                .Concat(withdrawalTransactions)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(Result<object>.SuccessResult(new
            {
                Summary = new
                {
                    TotalRevenue = totalRevenue,
                    TotalSessionBookings = totalSessionBookings,
                    TotalWithdrawals = totalWithdrawals,
                    TotalPlatformFees = totalPlatformFees,
                    TotalTutorEarnings = totalTutorEarnings,
                    NetProfit = totalPlatformFees
                },
                Transactions = allRecentTransactions,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = paymentTransactions.Count + withdrawalTransactions.Count
                }
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Result.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}"));
        }
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

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; } = "Student";
}
