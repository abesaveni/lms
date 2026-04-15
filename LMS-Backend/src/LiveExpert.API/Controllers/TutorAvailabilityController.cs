using LiveExpert.Application.Features.TutorAvailability.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 6: Tutor availability slots

[Route("api/tutor-availability")]
[Authorize]
public class TutorAvailabilityController : BaseController
{
    public TutorAvailabilityController(IMediator mediator) : base(mediator) { }

    [HttpGet("{tutorId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailability(Guid tutorId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTutorAvailabilityQuery { TutorId = tutorId }, ct);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpDelete("{slotId}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> DeleteSlot(Guid slotId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteAvailabilitySlotCommand { SlotId = slotId }, ct);
        return HandleResult(result);
    }
}
