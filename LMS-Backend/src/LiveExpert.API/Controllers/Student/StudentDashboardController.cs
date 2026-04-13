using LiveExpert.Application.Features.Dashboards.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Student;

/// <summary>
/// Student dashboard endpoints
/// </summary>
[Route("api/student/dashboard")]
[Authorize(Roles = "Student")]
[ApiController]
public class StudentDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get student dashboard overview
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(StudentDashboardDto), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var query = new GetStudentDashboardQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get student statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(StudentStatsDto), 200)]
    public async Task<IActionResult> GetStats()
    {
        var query = new GetStudentStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
