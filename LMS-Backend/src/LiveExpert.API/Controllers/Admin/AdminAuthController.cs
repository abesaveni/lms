using LiveExpert.Application.Features.Auth.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin authentication and user management endpoints
/// </summary>
[Route("api/admin")]
[ApiController]
public class AdminAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRepository<User> _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminAuthController(
        IMediator mediator,
        IRepository<User> userRepository,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Admin login (No public registration for admins)
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command);
        
        if (result.Success && result.Data != null)
        {
            return Ok(result);
        }
        
        return Unauthorized(result);
    }

    /// <summary>
    /// Create new admin user (Super Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("users/create-admin")]
    [ProducesResponseType(typeof(CreateAdminResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        // Verify current user is admin (additional security check)
        var currentUserId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(new { success = false, message = "Unauthorized" });
        }

        // Check if email already exists
        var existingUser = await _userRepository.FirstOrDefaultAsync(
            u => u.Email == request.Email, CancellationToken.None);

        if (existingUser != null)
        {
            return BadRequest(new 
            { 
                success = false, 
                message = "User with this email already exists" 
            });
        }

        // Create new admin user
        var newAdmin = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _encryptionService.Hash(request.Password),
            Role = UserRole.Admin,
            IsEmailVerified = true, // Admins are pre-verified
            IsPhoneVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newAdmin, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        var response = new CreateAdminResponse
        {
            Id = newAdmin.Id,
            Username = newAdmin.Username,
            Email = newAdmin.Email,
            PhoneNumber = newAdmin.PhoneNumber,
            CreatedAt = newAdmin.CreatedAt
        };

        return CreatedAtAction(nameof(CreateAdmin), new { id = newAdmin.Id }, new
        {
            success = true,
            data = response,
            message = "Admin user created successfully"
        });
    }

    /// <summary>
    /// Admin forgot password
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Admin reset password
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Admin change password
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("change-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Admin logout
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Refresh admin token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public class AdminLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateAdminRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class CreateAdminResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
