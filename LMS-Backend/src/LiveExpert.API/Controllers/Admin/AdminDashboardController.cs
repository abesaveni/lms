using LiveExpert.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// Admin dashboard endpoint
/// </summary>
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get admin dashboard overview
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminDashboardDto), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var query = new GetAdminDashboardQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
