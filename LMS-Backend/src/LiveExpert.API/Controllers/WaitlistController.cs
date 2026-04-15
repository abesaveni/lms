using LiveExpert.Application.Features.Waitlist.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 8: Waitlist for full group sessions

[Route("api/sessions/{sessionId}/waitlist")]
[Authorize]
public class WaitlistController : BaseController
{
    public WaitlistController(IMediator mediator) : base(mediator) { }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> JoinWaitlist(Guid sessionId, CancellationToken ct)
    {
        var result = await _mediator.Send(new JoinWaitlistCommand { SessionId = sessionId }, ct);
        return HandleResult(result);
    }

    [HttpGet("position")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetPosition(Guid sessionId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWaitlistPositionQuery { SessionId = sessionId }, ct);
        return HandleResult(result);
    }
}
