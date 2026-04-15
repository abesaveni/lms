using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Google Calendar OAuth for ALL users (Students and Tutors)
/// Mandatory for using the platform
/// </summary>
[Authorize]
[Route("api/calendar/oauth")]
[ApiController]
public class CalendarOAuthController : ControllerBase
{
    private readonly ICalendarConnectionService _calendarConnectionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;

    public CalendarOAuthController(
        ICalendarConnectionService calendarConnectionService,
        ICurrentUserService currentUserService,
        IRepository<User> userRepository)
    {
        _calendarConnectionService = calendarConnectionService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get Google Calendar OAuth authorization URL for current user
    /// </summary>
    [HttpGet("authorize")]
    [ProducesResponseType(typeof(Result<string>), 200)]
    public async Task<IActionResult> GetAuthorizationUrl([FromQuery] string? redirectUri)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<string>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound(Result<string>.FailureResult("NOT_FOUND", "User not found"));
        }

        var scheme = Request.Headers.ContainsKey("X-Forwarded-Proto")
            ? Request.Headers["X-Forwarded-Proto"].ToString().Split(",")[0].Trim()
            : Request.Scheme;
        var frontendRedirectUri = redirectUri ?? $"{scheme}://{Request.Host}/calendar/connect";
        var backendCallbackUrl = $"{scheme}://{Request.Host}/api/calendar/oauth/callback";

        var authUrl = await _calendarConnectionService.GetAuthorizationUrlAsync(
            userId.Value,
            frontendRedirectUri,
            backendCallbackUrl);

        if (string.IsNullOrEmpty(authUrl))
        {
            return BadRequest(Result<string>.FailureResult("CONFIG_MISSING", "Google Calendar is not fully configured by the administrator."));
        }

        return Ok(Result<string>.SuccessResult(authUrl));
    }

    /// <summary>
    /// Handle Google Calendar OAuth callback
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
            // Decode state to get userId and redirectUri
            var stateBytes = Convert.FromBase64String(state);
            var stateString = System.Text.Encoding.UTF8.GetString(stateBytes);
            var parts = stateString.Split('|');
            
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var userId))
            {
                return BadRequest(Result<bool>.FailureResult("INVALID_STATE", "Invalid state parameter"));
            }

            var frontendRedirectUri = parts[1];
            var callbackScheme = Request.Headers.ContainsKey("X-Forwarded-Proto")
                ? Request.Headers["X-Forwarded-Proto"].ToString().Split(",")[0].Trim()
                : Request.Scheme;
            var backendCallbackUrl = $"{callbackScheme}://{Request.Host}/api/calendar/oauth/callback";
            var success = await _calendarConnectionService.ExchangeCodeForTokensAsync(userId, code, backendCallbackUrl);

            if (success)
            {
                // Track Google Calendar consent
                try
                {
                    var consentRepository = HttpContext.RequestServices.GetRequiredService<IRepository<UserConsent>>();
                    var unitOfWork = HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
                    
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                    
                    var existingConsent = await consentRepository.FirstOrDefaultAsync(
                        c => c.UserId == userId && c.ConsentType == ConsentType.GoogleCalendar);
                    
                    if (existingConsent != null)
                    {
                        existingConsent.Granted = true;
                        existingConsent.GrantedAt = DateTime.UtcNow;
                        existingConsent.RevokedAt = null;
                        existingConsent.IpAddress = ipAddress;
                        existingConsent.UserAgent = userAgent;
                        await consentRepository.UpdateAsync(existingConsent);
                    }
                    else
                    {
                        var newConsent = new UserConsent
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            ConsentType = ConsentType.GoogleCalendar,
                            Granted = true,
                            GrantedAt = DateTime.UtcNow,
                            IpAddress = ipAddress,
                            UserAgent = userAgent,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await consentRepository.AddAsync(newConsent);
                    }
                    
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the OAuth flow
                }
                
                // Redirect to frontend success page
                return Redirect($"{frontendRedirectUri}?success=true");
            }

            return Redirect($"{frontendRedirectUri}?success=false&error=token_exchange_failed");
        }
        catch (Exception ex)
        {
            return BadRequest(Result<bool>.FailureResult("ERROR", ex.Message));
        }
    }

    /// <summary>
    /// Check if Google Calendar is connected for current user (MANDATORY for Students/Tutors, optional for Admins)
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

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return Unauthorized(Result<bool>.FailureResult("UNAUTHORIZED", "User not found"));
        }

        // Admins don't need calendar connection
        if (user.Role == UserRole.Admin)
        {
            return Ok(Result<bool>.SuccessResult(true));
        }

        var isConnected = await _calendarConnectionService.IsCalendarConnectedAsync(userId.Value);
        return Ok(Result<bool>.SuccessResult(isConnected));
    }

    /// <summary>
    /// Disconnect Google Calendar for current user
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

        var success = await _calendarConnectionService.RevokeConnectionAsync(userId.Value);
        return Ok(Result<bool>.SuccessResult(success));
    }

    /// <summary>
    /// Mock connect for development/testing when ClientId is missing
    /// </summary>
    [HttpPost("mock-connect")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> MockConnect()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result<bool>.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var connection = await _userRepository.GetByIdAsync(userId.Value);
        if (connection == null) return NotFound();

        // Check if already connected
        var existing = await _calendarConnectionService.IsCalendarConnectedAsync(userId.Value);
        if (existing) return Ok(Result<bool>.SuccessResult(true));

        // Create a fake connection
        var db = HttpContext.RequestServices.GetRequiredService<LiveExpert.Infrastructure.Data.ApplicationDbContext>();
        var encryption = HttpContext.RequestServices.GetRequiredService<IEncryptionService>();
        
        var newConnection = new UserCalendarConnection
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            Provider = CalendarProvider.Google,
            AccessToken = encryption.Encrypt("mock_access_token"),
            RefreshToken = encryption.Encrypt("mock_refresh_token"),
            TokenExpiry = DateTime.UtcNow.AddYears(1),
            GoogleEmail = "demo@liveexpert.ai",
            IsActive = true,
            ConnectedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.UserCalendarConnections.Add(newConnection);

        // Also save GoogleCalendar consent so the settings page shows "Connected"
        var consentRepository = HttpContext.RequestServices.GetRequiredService<IRepository<UserConsent>>();
        var unitOfWork = HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var existingConsent = await consentRepository.FirstOrDefaultAsync(
            c => c.UserId == userId.Value && c.ConsentType == ConsentType.GoogleCalendar);
        if (existingConsent != null)
        {
            existingConsent.Granted = true;
            existingConsent.GrantedAt = DateTime.UtcNow;
            existingConsent.RevokedAt = null;
            await consentRepository.UpdateAsync(existingConsent);
        }
        else
        {
            var newConsent = new UserConsent
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                ConsentType = ConsentType.GoogleCalendar,
                Granted = true,
                GrantedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await consentRepository.AddAsync(newConsent);
        }

        await db.SaveChangesAsync();
        await unitOfWork.SaveChangesAsync();

        return Ok(Result<bool>.SuccessResult(true));
    }
}
