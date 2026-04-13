using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Disputes.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Shared;

/// <summary>
/// Dispute resolution endpoints (accessible by all authenticated users)
/// </summary>
[Route("api/shared/disputes")]
[Authorize]
[ApiController]
public class DisputesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DisputesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new dispute
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<Guid>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateDispute([FromBody] CreateDisputeCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.Success)
            return CreatedAtAction(nameof(GetDisputes), new { id = result.Data }, result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get my disputes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DisputeDto>), 200)]
    public async Task<IActionResult> GetDisputes()
    {
        var query = new GetDisputesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Respond to a dispute (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/respond")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RespondToDispute(Guid id, [FromBody] RespondToDisputeRequest request)
    {
        var command = new RespondToDisputeCommand 
        { 
            DisputeId = id,
            Response = request.Response
        };
        var result = await _mediator.Send(command);
        
        if (result.Success)
            return Ok(result);
        
        return NotFound(result);
    }
}

public class RespondToDisputeRequest
{
    public string Response { get; set; } = string.Empty;
}
