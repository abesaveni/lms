using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Consents.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

/// <summary>
/// Cookie consent management endpoints (public for anonymous users, authenticated for logged-in users)
/// </summary>
[Route("api/consent/cookies")]
[ApiController]
public class CookieConsentController : BaseController
{
    public CookieConsentController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Save cookie consent (works for both anonymous and authenticated users)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CookieConsentDto), 200)]
    public async Task<IActionResult> SaveConsent([FromBody] SaveCookieConsentRequest request)
    {
        var command = new SaveCookieConsentCommand
        {
            Necessary = true, // Always true
            Functional = request.Functional,
            Analytics = request.Analytics,
            Marketing = request.Marketing,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Update cookie consent preferences
    /// </summary>
    [HttpPut]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CookieConsentDto), 200)]
    public async Task<IActionResult> UpdateConsent([FromBody] UpdateCookieConsentRequest request)
    {
        var command = new UpdateCookieConsentCommand
        {
            Functional = request.Functional,
            Analytics = request.Analytics,
            Marketing = request.Marketing
        };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get current user's cookie consent (returns null for anonymous users)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CookieConsentDto), 200)]
    public async Task<IActionResult> GetConsent()
    {
        var query = new GetCookieConsentQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}

public class SaveCookieConsentRequest
{
    public bool Functional { get; set; }
    public bool Analytics { get; set; }
    public bool Marketing { get; set; }
}

public class UpdateCookieConsentRequest
{
    public bool Functional { get; set; }
    public bool Analytics { get; set; }
    public bool Marketing { get; set; }
}

/// <summary>
/// Google OAuth consent management endpoints (authenticated users only)
/// </summary>
[Route("api/consent/user")]
[Authorize]
[ApiController]
public class UserConsentController : BaseController
{
    public UserConsentController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Save Google OAuth consent (Login or Calendar)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserConsentDto), 200)]
    public async Task<IActionResult> SaveConsent([FromBody] SaveUserConsentRequest request)
    {
        var command = new SaveUserConsentCommand
        {
            ConsentType = request.ConsentType,
            Granted = request.Granted,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Revoke Google OAuth consent
    /// </summary>
    [HttpPost("revoke")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RevokeConsent([FromBody] RevokeUserConsentRequest request)
    {
        var command = new RevokeUserConsentCommand
        {
            ConsentType = request.ConsentType
        };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all consents for current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserConsentDto>), 200)]
    public async Task<IActionResult> GetConsents()
    {
        var query = new GetUserConsentsQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}

public class SaveUserConsentRequest
{
    public Domain.Enums.ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }
}

public class RevokeUserConsentRequest
{
    public Domain.Enums.ConsentType ConsentType { get; set; }
}

/// <summary>
/// Admin endpoints for viewing all consents (audit trail)
/// </summary>
[Route("api/admin/consents")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminConsentController : BaseController
{
    public AdminConsentController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all cookie consents (admin only)
    /// </summary>
    [HttpGet("cookies")]
    [ProducesResponseType(typeof(PaginatedResult<CookieConsentAdminDto>), 200)]
    public async Task<IActionResult> GetCookieConsents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? userId = null)
    {
        var query = new GetCookieConsentsQuery
        {
            Page = page,
            PageSize = pageSize,
            UserId = userId
        };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all user consents (Google OAuth) (admin only)
    /// </summary>
    [HttpGet("user")]
    [ProducesResponseType(typeof(PaginatedResult<UserConsentAdminDto>), 200)]
    public async Task<IActionResult> GetUserConsents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? userId = null,
        [FromQuery] Domain.Enums.ConsentType? consentType = null)
    {
        var query = new GetUserConsentsAdminQuery
        {
            Page = page,
            PageSize = pageSize,
            UserId = userId,
            ConsentType = consentType
        };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
