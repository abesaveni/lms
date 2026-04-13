using LiveExpert.Application.Features.Reviews.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ReviewsController : BaseController
{
    public ReviewsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Submit a review for a tutor (Student only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get reviews (optionally filtered by tutor)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews([FromQuery] GetReviewsQuery query)
    {
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Respond to a review (Tutor only)
    /// </summary>
    [HttpPost("{id}/respond")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> RespondToReview(Guid id, [FromBody] RespondToReviewRequest request)
    {
        var command = new RespondToReviewCommand 
        { 
            ReviewId = id,
            Response = request.Response
        };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}

public class RespondToReviewRequest
{
    public string Response { get; set; } = string.Empty;
}
