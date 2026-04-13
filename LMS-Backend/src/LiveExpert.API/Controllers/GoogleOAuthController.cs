using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LiveExpert.Infrastructure.Repositories;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Google OAuth integration for tutors to connect Google Calendar
/// </summary>
[Authorize(Roles = "Tutor")]
[Route("api/google/oauth")]
[ApiController]
public class GoogleOAuthController : ControllerBase
{
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;

    public GoogleOAuthController(
        IGoogleOAuthService googleOAuthService,
        ICurrentUserService currentUserService,
        IRepository<User> userRepository)
    {
        _googleOAuthService = googleOAuthService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get Google OAuth authorization URL for current tutor
    /// </summary>
    [HttpGet("authorize")]
    [ProducesResponseType(typeof(Result<string>), 200)]
    public async Task<IActionResult> GetAuthorizationUrl([FromQuery] string redirectUri)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<string>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null || user.Role != UserRole.Tutor)
        {
            return Forbid();
        }

        var authUrl = await _googleOAuthService.GetAuthorizationUrlAsync(
            userId.Value,
            redirectUri ?? $"{Request.Scheme}://{Request.Host}/api/google/oauth/callback");

        return Ok(Result<string>.SuccessResult(authUrl));
    }

    /// <summary>
    /// Handle Google OAuth callback
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous] // Public endpoint for OAuth callback
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> HandleCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(Result<bool>.FailureResult("OAUTH_ERROR", $"OAuth error: {error}"));
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return BadRequest(Result<bool>.FailureResult("INVALID_REQUEST", "Missing code or state parameter"));
        }

        try
        {
            // Decode state to get tutorId and redirectUri
            var stateBytes = Convert.FromBase64String(state);
            var stateString = System.Text.Encoding.UTF8.GetString(stateBytes);
            var parts = stateString.Split('|');
            
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var tutorId))
            {
                return BadRequest(Result<bool>.FailureResult("INVALID_STATE", "Invalid state parameter"));
            }

            var redirectUri = parts[1];
            var success = await _googleOAuthService.ExchangeCodeForTokensAsync(tutorId, code, redirectUri);

            if (success)
            {
                // Redirect to frontend success page
                return Redirect($"{redirectUri}?success=true");
            }

            return Redirect($"{redirectUri}?success=false&error=token_exchange_failed");
        }
        catch (Exception ex)
        {
            return BadRequest(Result<bool>.FailureResult("ERROR", ex.Message));
        }
    }

    /// <summary>
    /// Check if Google Calendar is connected for current tutor
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> GetConnectionStatus()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<bool>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var isConnected = await _googleOAuthService.IsGoogleCalendarConnectedAsync(userId.Value);
        return Ok(Result<bool>.SuccessResult(isConnected));
    }

    /// <summary>
    /// Disconnect Google Calendar for current tutor
    /// </summary>
    [HttpPost("disconnect")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> Disconnect()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<bool>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var success = await _googleOAuthService.RevokeTokensAsync(userId.Value);
        return Ok(Result<bool>.SuccessResult(success));
    }
}
