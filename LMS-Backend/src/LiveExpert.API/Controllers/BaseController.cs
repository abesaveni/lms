using LiveExpert.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator _mediator;

    protected BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.Success)
        {
            return Ok(result);
        }

        return result.Error?.Code switch
        {
            "NOT_FOUND" => NotFound(result),
            "UNAUTHORIZED" => Unauthorized(result),
            "FORBIDDEN" => StatusCode(403, result),
            "VALIDATION_ERROR" => BadRequest(result),
            "CONFLICT" => Conflict(result),
            _ => BadRequest(result)
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.Success)
        {
            return Ok(result);
        }

        return result.Error?.Code switch
        {
            "NOT_FOUND" => NotFound(result),
            "UNAUTHORIZED" => Unauthorized(result),
            "FORBIDDEN" => StatusCode(403, result),
            "VALIDATION_ERROR" => BadRequest(result),
            "CONFLICT" => Conflict(result),
            _ => BadRequest(result)
        };
    }

    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    protected string? GetCurrentUserRole()
    {
        return User.FindFirst("role")?.Value;
    }
}
