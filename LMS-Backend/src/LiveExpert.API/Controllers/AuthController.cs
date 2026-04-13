using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Unified authentication endpoints (auto-detects role)
/// </summary>
[Route("api/auth")]
[ApiController]
[EnableCors("AllowAllDev")] // Explicitly enable CORS for this controller
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Handle OPTIONS preflight request
    /// </summary>
    [HttpOptions("login")]
    public IActionResult OptionsLogin()
    {
        return Ok();
    }

    /// <summary>
    /// Unified login - automatically detects user role from email
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<LoginResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] UnifiedLoginRequest request)
    {
        // SELF-HEALING: Ensure Columns exist (Professional Reliability)
        try {
            var db = _mediator as dynamic; 
            // This is a bit tricky with mediator, better use a direct DB check if possible or just rely on the specialized handlers.
            // Actually, I'll put it in the ForgotPassword handler since that's where it's first needed.
        } catch { }

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
    /// Handle OPTIONS preflight for forgot password
    /// </summary>
    [HttpOptions("forgot-password")]
    public IActionResult OptionsForgotPassword()
    {
        return Ok();
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    /// <summary>
    /// Handle OPTIONS preflight for reset password
    /// </summary>
    [HttpOptions("reset-password")]
    public IActionResult OptionsResetPassword()
    {
        return Ok();
    }

    /// <summary>
    /// Reset password using token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}

public class UnifiedLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}




