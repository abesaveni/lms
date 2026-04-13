using LiveExpert.Application.Features.Dashboards.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Tutor;

/// <summary>
/// Tutor dashboard endpoints
/// </summary>
[Route("api/tutor/dashboard")]
[Authorize(Roles = "Tutor")]
[ApiController]
public class TutorDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public TutorDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get tutor dashboard overview
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TutorDashboardDto), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var query = new GetTutorDashboardQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get tutor statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(TutorStatsDto), 200)]
    public async Task<IActionResult> GetStats()
    {
        var query = new GetTutorStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
