using LiveExpert.Application.Features.Inquiries.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 12: Pre-booking inquiry

[Route("api/inquiries")]
[Authorize]
public class InquiriesController : BaseController
{
    public InquiriesController(IMediator mediator) : base(mediator) { }

    [HttpGet("sent")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetSent(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSentInquiriesQuery(), ct);
        return HandleResult(result);
    }

    [HttpGet("received")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GetReceived(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetReceivedInquiriesQuery(), ct);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Send([FromBody] SendInquiryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("{inquiryId}/reply")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> Reply(Guid inquiryId, [FromBody] ReplyToInquiryCommand command, CancellationToken ct)
    {
        command.InquiryId = inquiryId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("{inquiryId}/close")]
    public async Task<IActionResult> Close(Guid inquiryId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CloseInquiryCommand { InquiryId = inquiryId }, ct);
        return HandleResult(result);
    }
}
