using LiveExpert.Application.Features.Assignments.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 3: Homework / Assignments

[Route("api/assignments")]
[Authorize]
public class AssignmentsController : BaseController
{
    public AssignmentsController(IMediator mediator) : base(mediator) { }

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSessionAssignments(Guid sessionId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionAssignmentsQuery { SessionId = sessionId }, ct);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("{assignmentId}/submit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitAssignment(Guid assignmentId, [FromBody] SubmitAssignmentCommand command, CancellationToken ct)
    {
        command.AssignmentId = assignmentId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpGet("{assignmentId}/submissions")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GetSubmissions(Guid assignmentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAssignmentSubmissionsQuery { AssignmentId = assignmentId }, ct);
        return HandleResult(result);
    }

    [HttpPost("submissions/{submissionId}/grade")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GradeSubmission(Guid submissionId, [FromBody] GradeSubmissionCommand command, CancellationToken ct)
    {
        command.SubmissionId = submissionId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }
}
