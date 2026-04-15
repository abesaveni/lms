using LiveExpert.Application.Features.Bundles.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 9: Session bundles

[Route("api/bundles")]
[Authorize]
public class BundlesController : BaseController
{
    public BundlesController(IMediator mediator) : base(mediator) { }

    [HttpGet("tutor/{tutorId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTutorBundles(Guid tutorId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTutorBundlesQuery { TutorId = tutorId }, ct);
        return HandleResult(result);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyBundles(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyBundlesQuery(), ct);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> CreateBundle([FromBody] CreateBundleCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("{bundleId}/purchase")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> PurchaseBundle(Guid bundleId, CancellationToken ct)
    {
        var result = await _mediator.Send(new PurchaseBundleCommand { BundleId = bundleId }, ct);
        return HandleResult(result);
    }

    [HttpPost("{bundleId}/activate")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> ActivatePurchase(Guid bundleId, [FromBody] ActivateBundlePurchaseCommand command, CancellationToken ct)
    {
        command.BundleId = bundleId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }
}
