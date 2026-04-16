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

    public AdminUsersController(
        ApplicationDbContext context,
        IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
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

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
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
            })
            .ToListAsync();

        // Attach tutor verificationStatus for Tutor-role users
        var tutorIds = users.Where(u => u.role == "Tutor")
            .Select(u => Guid.Parse(u.id)).ToList();

        var tutorStatuses = tutorIds.Any()
            ? await _context.TutorProfiles
                .Where(t => tutorIds.Contains(t.UserId))
                .Select(t => new { t.UserId, status = t.VerificationStatus.ToString() })
                .ToDictionaryAsync(t => t.UserId, t => t.status)
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
    /// Delete a user account
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(Result.FailureResult("NOT_FOUND", "User not found"));

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(Result.SuccessResult("User deleted"));
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
