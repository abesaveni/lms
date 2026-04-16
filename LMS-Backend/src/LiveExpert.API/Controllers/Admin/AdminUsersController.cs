using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin user management
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
[ApiController]
public class AdminUsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        ApplicationDbContext context,
        IEncryptionService encryptionService,
        ILogger<AdminUsersController> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of users with optional role filter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
            query = query.Where(u => u.Role == parsedRole);

        var totalRecords = await query.CountAsync();

        // Materialize raw DB rows first (no .ToString() inside EF Core SQL)
        var rawUsers = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.PhoneNumber,
                u.Role,
                u.IsActive,
                u.IsEmailVerified,
                u.IsPhoneVerified,
                u.CreatedAt,
                u.LastLoginAt
            })
            .ToListAsync();

        // Convert to the response shape in memory
        var users = rawUsers.Select(u => new
        {
            id              = u.Id.ToString(),
            username        = u.Username,
            email           = u.Email,
            phoneNumber     = u.PhoneNumber,
            role            = u.Role.ToString(),
            isActive        = u.IsActive,
            isEmailVerified = u.IsEmailVerified,
            isPhoneVerified = u.IsPhoneVerified,
            createdAt       = u.CreatedAt,
            lastLoginAt     = u.LastLoginAt
        }).ToList();

        // Attach tutor verificationStatus for Tutor-role users
        var tutorIds = users.Where(u => u.role == "Tutor")
            .Select(u => Guid.Parse(u.id)).ToList();

        // Materialize TutorProfiles without .ToString() in EF Core SQL
        var tutorStatuses = tutorIds.Any()
            ? (await _context.TutorProfiles
                .Where(t => tutorIds.Contains(t.UserId))
                .Select(t => new { t.UserId, t.VerificationStatus })
                .ToListAsync())
                .ToDictionary(t => t.UserId, t => t.VerificationStatus.ToString())
            : new Dictionary<Guid, string>();

        var result = users.Select(u => new
        {
            u.id,
            u.username,
            u.email,
            u.phoneNumber,
            u.role,
            u.isActive,
            u.isEmailVerified,
            u.isPhoneVerified,
            u.createdAt,
            u.lastLoginAt,
            verificationStatus = u.role == "Tutor" && tutorStatuses.TryGetValue(Guid.Parse(u.id), out var vs) ? vs : (string?)null
        }).ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                items = result,
                pagination = new
                {
                    currentPage  = page,
                    pageSize,
                    totalRecords,
                    totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize)
                }
            }
        });
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(Result.FailureResult("EMAIL_EXISTS", "A user with this email already exists"));

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(Result.FailureResult("USERNAME_EXISTS", "A user with this username already exists"));

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            return BadRequest(Result.FailureResult("INVALID_ROLE", "Invalid role"));

        var password = request.Password ?? Guid.NewGuid().ToString("N").Substring(0, 12);

        var user = new User
        {
            Id              = Guid.NewGuid(),
            Username        = request.Username,
            Email           = request.Email,
            PhoneNumber     = request.PhoneNumber ?? string.Empty,
            FirstName       = request.FirstName ?? string.Empty,
            LastName        = request.LastName ?? string.Empty,
            PasswordHash    = _encryptionService.Hash(password),
            Role            = role,
            IsActive        = true,
            IsEmailVerified = false,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create role-specific profile
        if (role == UserRole.Tutor)
        {
            _context.TutorProfiles.Add(new TutorProfile
            {
                Id                 = Guid.NewGuid(),
                UserId             = user.Id,
                VerificationStatus = VerificationStatus.Pending,
                IsVisible          = false,
                CreatedAt          = DateTime.UtcNow,
                UpdatedAt          = DateTime.UtcNow
            });
        }
        else if (role == UserRole.Student)
        {
            _context.StudentProfiles.Add(new StudentProfile
            {
                Id        = Guid.NewGuid(),
                UserId    = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return Ok(Result.SuccessResult("User created successfully"));
    }

    /// <summary>
    /// Activate a user account
    /// </summary>
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

        user.IsActive   = true;
        user.UpdatedAt  = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(Result.SuccessResult("User activated"));
    }

    /// <summary>
    /// Deactivate a user account
    /// </summary>
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

        user.IsActive  = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(Result.SuccessResult("User deactivated"));
    }

    /// <summary>
    /// Delete a user account and all associated data
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.TutorProfile)
                .Include(u => u.StudentProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

            if (user.Email == "superadmin@liveexpert.ai")
                return BadRequest(Result.FailureResult("FORBIDDEN", "Super admin cannot be deleted"));

            _logger.LogInformation("Hard deleting user {UserId} and all dependencies", id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Communication & Social
                _context.Messages.RemoveRange(_context.Messages.Where(m => m.SenderId == id || m.ReceiverId == id));
                _context.Conversations.RemoveRange(_context.Conversations.Where(c => c.User1Id == id || c.User2Id == id));
                _context.Reviews.RemoveRange(_context.Reviews.Where(r => r.StudentId == id || r.TutorId == id));
                _context.TutorFollowers.RemoveRange(_context.TutorFollowers.Where(f => f.TutorId == id || f.StudentId == id));
                _context.ChatRequests.RemoveRange(_context.ChatRequests.Where(c => c.StudentId == id || c.TutorId == id || (c.LastActionById != null && c.LastActionById == id)));

                // Sessions & Bookings
                var userSessionIds = await _context.Sessions.Where(s => s.TutorId == id).Select(s => s.Id).ToListAsync();
                await _context.SessionBookings.Where(b => b.StudentId == id || userSessionIds.Contains(b.SessionId)).ExecuteDeleteAsync();
                _context.SessionMeetLinks.RemoveRange(_context.SessionMeetLinks.Where(l => userSessionIds.Contains(l.SessionId)));
                _context.VirtualClassroomSessions.RemoveRange(_context.VirtualClassroomSessions.Where(v => v.TutorId == id));
                _context.Sessions.RemoveRange(_context.Sessions.Where(s => s.TutorId == id));

                // Courses & Enrollments
                _context.TrialSessions.RemoveRange(_context.TrialSessions.Where(t => t.TutorId == id || t.StudentId == id));
                _context.CourseEnrollments.RemoveRange(_context.CourseEnrollments.Where(e => e.StudentId == id));
                var userCourseIds = await _context.Courses.Where(c => c.TutorId == id).Select(c => c.Id).ToListAsync();
                if (userCourseIds.Any())
                {
                    _context.CourseEnrollments.RemoveRange(_context.CourseEnrollments.Where(e => userCourseIds.Contains(e.CourseId)));
                    _context.CourseSessions.RemoveRange(_context.CourseSessions.Where(cs => userCourseIds.Contains(cs.CourseId)));
                    _context.Courses.RemoveRange(_context.Courses.Where(c => c.TutorId == id));
                }

                // Financials
                _context.Payments.RemoveRange(_context.Payments.Where(p => p.StudentId == id || p.TutorId == id));
                _context.WithdrawalRequests.RemoveRange(_context.WithdrawalRequests.Where(w => w.UserId == id || (w.ProcessedBy != null && w.ProcessedBy == id)));
                _context.PayoutRequests.RemoveRange(_context.PayoutRequests.Where(p => p.TutorId == id || (p.ProcessedBy != null && p.ProcessedBy == id)));
                _context.TutorEarnings.RemoveRange(_context.TutorEarnings.Where(e => e.TutorId == id));
                _context.BankAccounts.RemoveRange(_context.BankAccounts.Where(b => b.UserId == id));

                // Tracking & Profile Extras
                _context.Referrals.RemoveRange(_context.Referrals.Where(r => r.ReferrerUserId == id || r.ReferredUserId == id));
                _context.KYCDocuments.RemoveRange(_context.KYCDocuments.Where(k => k.UserId == id || (k.VerifiedBy != null && k.VerifiedBy == id)));
                _context.Notifications.RemoveRange(_context.Notifications.Where(n => n.UserId == id));
                _context.UserConsents.RemoveRange(_context.UserConsents.Where(c => c.UserId == id));
                _context.CookieConsents.RemoveRange(_context.CookieConsents.Where(c => c.UserId == id));
                _context.BonusPoints.RemoveRange(_context.BonusPoints.Where(b => b.UserId == id));
                _context.UserNotificationPreferences.RemoveRange(_context.UserNotificationPreferences.Where(p => p.UserId == id));

                // Admin & System
                _context.AuditLogs.RemoveRange(_context.AuditLogs.Where(a => a.UserId == id));
                _context.WhatsAppCampaigns.RemoveRange(_context.WhatsAppCampaigns.Where(w => w.CreatedBy == id));
                _context.AdminPermissions.RemoveRange(_context.AdminPermissions.Where(p => p.AdminId == id || (p.GrantedBy != null && p.GrantedBy == id)));

                await _context.APIKeys.Where(k => k.UpdatedBy == id).ExecuteUpdateAsync(s => s.SetProperty(k => k.UpdatedBy, (Guid?)null));
                await _context.SystemSettings.Where(s => s.UpdatedBy == id).ExecuteUpdateAsync(s => s.SetProperty(s => s.UpdatedBy, (Guid?)null));

                _context.TutorGoogleTokens.RemoveRange(_context.TutorGoogleTokens.Where(t => t.TutorId == id));
                _context.UserCalendarConnections.RemoveRange(_context.UserCalendarConnections.Where(c => c.UserId == id));

                if (user.TutorProfile != null)
                {
                    _context.TutorVerifications.RemoveRange(_context.TutorVerifications.Where(v => v.TutorId == id));
                    _context.TutorProfiles.Remove(user.TutorProfile);
                }

                if (user.StudentProfile != null)
                    _context.StudentProfiles.Remove(user.StudentProfile);

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully purged user {UserId} from all system tables.", id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Universal purge failed for user {UserId}", id);
                throw;
            }

            return Ok(Result.SuccessResult("User and all associated data deleted permanently"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hard delete user {UserId}", id);
            return StatusCode(500, Result.FailureResult("SERVER_ERROR", $"Deep delete failed. Error: {ex.Message}. Inner: {ex.InnerException?.Message}"));
        }
    }
}

public class CreateUserRequest
{
    public string Username    { get; set; } = string.Empty;
    public string Email       { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role        { get; set; } = "Student";
    public string? Password   { get; set; }
    public string? FirstName  { get; set; }
    public string? LastName   { get; set; }
}
