using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Auth.Commands;
using LiveExpert.Application.Features.Consents.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveExpert.API.Controllers.Student;

/// <summary>
/// Student authentication endpoints
/// </summary>
[Route("api/student")]
[ApiController]
public class StudentAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<StudentAuthController> _logger;

    public StudentAuthController(
        IMediator mediator,
        IEmailService emailService,
        ICacheService cacheService,
        ILogger<StudentAuthController> logger)
    {
        _mediator = mediator;
        _emailService = emailService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new student account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] StudentRegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            PhoneNumber = request.PhoneNumber,
            WhatsAppNumber = request.WhatsAppNumber,
            Role = Domain.Enums.UserRole.Student,
            FirstName = request.FirstName,
            LastName = request.LastName,
            SignupVerificationToken = request.SignupVerificationToken
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
            return CreatedAtAction(nameof(Register), result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Student login
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] StudentLoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command);
        
        // Verify the user is actually a student
        if (result.Success && result.Data != null)
        {
            // Role verification happens in the handler
            return Ok(result);
        }
        
        return Unauthorized(result);
    }

    /// <summary>
    /// Student Google OAuth login/register
    /// </summary>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand command)
    {
        // Set role to Student for OAuth registration
        command.Role = Domain.Enums.UserRole.Student;
        var result = await _mediator.Send(command);
        
        // Track Google Login consent after successful login
        if (result.Success && result.Data != null)
        {
            try
            {
                var consentMediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
                var saveConsentCommand = new SaveUserConsentCommand
                {
                    ConsentType = ConsentType.GoogleLogin,
                    Granted = true,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                };
                await consentMediator.Send(saveConsentCommand);
            }
            catch (Exception ex)
            {
                // Log but don't fail login
                System.Diagnostics.Debug.WriteLine($"Failed to track Google login consent: {ex.Message}");
            }
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Verify student email
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Request email verification code for signup (student)
    /// </summary>
    [HttpPost("request-email-verification")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RequestEmailVerification([FromBody] RequestSignupEmailVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(Result.FailureResult("INVALID_EMAIL", "Email is required"));
        }

        var code = new Random().Next(100000, 999999).ToString();
        var cacheKey = $"signup-email:{request.Email.Trim().ToLowerInvariant()}";
        var entry = new SignupEmailVerificationState
        {
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Verified = false
        };
        await _cacheService.SetAsync(cacheKey, entry, TimeSpan.FromMinutes(10));

        // Always log the code so it's visible in the backend console during development
        _logger.LogWarning(">>> SIGNUP OTP for {Email}: {Code} (valid 10 min) <<<", request.Email, code);

        var emailSent = false;
        try
        {
            await _emailService.SendEmailAsync(
                request.Email,
                "Verify your email",
                EmailTemplates.VerificationEmail(request.Name ?? string.Empty, code, 10)
            );
            emailSent = true;
        }
        catch { /* logged inside EmailService */ }

        var message = emailSent
            ? "Verification code sent to your email"
            : "Email delivery failed — your OTP is shown on screen";

        // Always return OTP on screen when email fails (temporary until SMTP is fixed)
        return Ok(new { success = true, message, devOtp = !emailSent ? code : null });
    }

    /// <summary>
    /// Confirm email verification code for signup (student)
    /// </summary>
    [HttpPost("confirm-email-verification")]
    [ProducesResponseType(typeof(Result<SignupVerificationResponse>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmEmailVerification([FromBody] ConfirmSignupEmailVerificationRequest request)
    {
        var cacheKey = $"signup-email:{request.Email.Trim().ToLowerInvariant()}";
        var entry = await _cacheService.GetAsync<SignupEmailVerificationState>(cacheKey);
        if (entry == null || entry.ExpiresAt < DateTime.UtcNow)
        {
            return BadRequest(Result.FailureResult("CODE_EXPIRED", "Verification code has expired"));
        }

        if (!string.Equals(entry.Code, request.Code, StringComparison.Ordinal))
        {
            return BadRequest(Result.FailureResult("INVALID_CODE", "Invalid verification code"));
        }

        entry.Verified = true;
        entry.VerificationToken = Guid.NewGuid().ToString("N");
        entry.ExpiresAt = DateTime.UtcNow.AddMinutes(30);
        await _cacheService.SetAsync(cacheKey, entry, TimeSpan.FromMinutes(30));

        return Ok(Result<SignupVerificationResponse>.SuccessResult(new SignupVerificationResponse
        {
            VerificationToken = entry.VerificationToken
        }));
    }

    /// <summary>
    /// Resend student email verification
    /// </summary>
    [HttpPost("resend-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResendEmail([FromBody] ResendEmailVerificationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Verify student WhatsApp
    /// </summary>
    [HttpPost("verify-whatsapp")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VerifyWhatsApp([FromBody] VerifyWhatsAppCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Student forgot password
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
    /// Student reset password
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
    /// Student change password
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpPost("change-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Student logout
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Refresh student token
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

public class StudentRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? SignupVerificationToken { get; set; }
}

public class RequestSignupEmailVerificationRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class ConfirmSignupEmailVerificationRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class SignupVerificationResponse
{
    public string VerificationToken { get; set; } = string.Empty;
}

public class StudentLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
