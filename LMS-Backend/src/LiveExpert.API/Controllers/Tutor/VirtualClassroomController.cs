using LiveExpert.Application.Common;
using LiveExpert.Application.Features.VirtualClassroom.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Tutor;

/// <summary>
/// Tutor virtual classroom endpoints
/// </summary>
[Route("api/tutor/virtual-classroom")]
[Authorize(Roles = "Tutor")]
[ApiController]
public class VirtualClassroomController : ControllerBase
{
    private readonly IMediator _mediator;

    public VirtualClassroomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Start virtual classroom session (15 min/day limit)
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(Result<VirtualClassroomDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> StartVirtualClassroom()
    {
        var command = new StartVirtualClassroomCommand();
        var result = await _mediator.Send(command);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// End virtual classroom session
    /// </summary>
    [HttpPost("{sessionId}/end")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> EndVirtualClassroom(Guid sessionId)
    {
        var command = new EndVirtualClassroomCommand { SessionId = sessionId };
        var result = await _mediator.Send(command);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}




