using LiveExpert.Application.Features.SubscriptionPlans.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 10: LMS Subscription Plans (distinct from the basic platform subscription in SubscriptionController)

[Route("api/subscription-plans")]
[Authorize]
public class SubscriptionPlansController : BaseController
{
    public SubscriptionPlansController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscriptionPlansQuery(), ct);
        return HandleResult(result);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySubscription(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMySubscriptionQuery(), ct);
        return HandleResult(result);
    }

    [HttpPost("{planId}/subscribe")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Subscribe(Guid planId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubscribeCommand { PlanId = planId }, ct);
        return HandleResult(result);
    }

    [HttpPost("{planId}/activate")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> ActivateSubscription(Guid planId, [FromBody] ActivateSubscriptionPlanCommand command, CancellationToken ct)
    {
        command.PlanId = planId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("cancel")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CancelSubscription(CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelSubscriptionCommand(), ct);
        return HandleResult(result);
    }
}
