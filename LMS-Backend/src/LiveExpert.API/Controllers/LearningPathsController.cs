using LiveExpert.Application.Features.LearningPaths.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 14: Learning paths

[Route("api/learning-paths")]
[Authorize]
public class LearningPathsController : BaseController
{
    public LearningPathsController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaths([FromQuery] Guid? tutorId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLearningPathsQuery { TutorId = tutorId }, ct);
        return HandleResult(result);
    }

    [HttpGet("{pathId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPath(Guid pathId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLearningPathQuery { PathId = pathId }, ct);
        return HandleResult(result);
    }

    [HttpGet("enrollments/mine")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyLearningEnrollmentsQuery(), ct);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> CreatePath([FromBody] CreateLearningPathCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("{pathId}/publish")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> PublishPath(Guid pathId, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishLearningPathCommand { PathId = pathId }, ct);
        return HandleResult(result);
    }

    [HttpPost("{pathId}/enroll")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Enroll(Guid pathId, CancellationToken ct)
    {
        var result = await _mediator.Send(new EnrollInLearningPathCommand { PathId = pathId }, ct);
        return HandleResult(result);
    }

    [HttpPost("enrollments/{enrollmentId}/complete-step")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CompleteStep(Guid enrollmentId, [FromBody] CompleteStepCommand command, CancellationToken ct)
    {
        command.EnrollmentId = enrollmentId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }
}
