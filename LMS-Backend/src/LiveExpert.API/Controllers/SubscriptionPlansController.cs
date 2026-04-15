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

    /// <summary>
    /// Feature 24: Cancel subscription — may return a retention offer instead of cancelling immediately.
    /// Check the response: if RetentionOfferMade=true, call /retention/accept or /retention/reject.
    /// </summary>
    [HttpPost("cancel")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>Feature 24: Accept the retention discount offer — creates a discounted renewal Razorpay order.</summary>
    [HttpPost("retention/accept")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> AcceptRetentionOffer(CancellationToken ct)
    {
        var result = await _mediator.Send(new AcceptRetentionOfferCommand(), ct);
        return HandleResult(result);
    }

    /// <summary>Feature 24: Reject the retention offer — cancels the subscription immediately.</summary>
    [HttpPost("retention/reject")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> RejectRetentionOffer(CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectRetentionOfferCommand(), ct);
        return HandleResult(result);
    }

    /// <summary>Feature 21: Enable or disable auto-renewal.</summary>
    [HttpPost("auto-renew")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SetAutoRenewal([FromBody] SetAutoRenewalCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Feature 25: Switch to a different plan with proration.
    /// Returns RequiresPayment=true for upgrades (create Razorpay order) or ImmediateSwitch=true for downgrades.
    /// </summary>
    [HttpPost("{newPlanId}/switch")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SwitchPlan(Guid newPlanId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SwitchSubscriptionPlanCommand { NewPlanId = newPlanId }, ct);
        return HandleResult(result);
    }

    /// <summary>Feature 25: Activate an upgrade after Razorpay payment.</summary>
    [HttpPost("{newPlanId}/switch/activate")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> ActivatePlanSwitch(Guid newPlanId, [FromBody] ActivatePlanSwitchCommand command, CancellationToken ct)
    {
        command.NewPlanId = newPlanId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }
}
