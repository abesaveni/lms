using LiveExpert.Application.Features.StudentRatings.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 4: Mutual rating - tutor rates student

[Route("api/student-ratings")]
[Authorize]
public class StudentRatingsController : BaseController
{
    public StudentRatingsController(IMediator mediator) : base(mediator) { }

    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> RateStudent([FromBody] RateStudentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetStudentRatings(Guid studentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentRatingsQuery { StudentId = studentId }, ct);
        return HandleResult(result);
    }
}
