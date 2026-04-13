using LiveExpert.Application.Common;
using LiveExpert.Application.Features.VirtualClassroom.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Shared;

/// <summary>
/// Public virtual classroom endpoints (for students to see active classrooms)
/// </summary>
[Route("api/shared/virtual-classrooms")]
[AllowAnonymous]
[ApiController]
public class VirtualClassroomController : ControllerBase
{
    private readonly IMediator _mediator;

    public VirtualClassroomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all active virtual classrooms (visible to students)
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(Result<List<ActiveVirtualClassroomDto>>), 200)]
    public async Task<IActionResult> GetActiveVirtualClassrooms()
    {
        var query = new GetActiveVirtualClassroomsQuery();
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}




