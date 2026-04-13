using LiveExpert.Application.Features.Sessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Tutor;

/// <summary>
/// Tutor sessions management endpoints
/// </summary>
[Route("api/tutor/sessions")]
[Authorize(Roles = "Tutor")]
[ApiController]
public class TutorSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TutorSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get my created sessions
    /// </summary>
    [HttpGet("my-sessions")]
    [ProducesResponseType(typeof(List<MySessionDto>), 200)]
    public async Task<IActionResult> GetMySessions()
    {
        var query = new GetMySessionsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
