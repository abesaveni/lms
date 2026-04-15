using LiveExpert.Application.Features.Sessions.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

// Feature 2: Session notes by tutor

[Route("api/sessions/{sessionId}/notes")]
[Authorize]
public class SessionNotesController : BaseController
{
    public SessionNotesController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> GetNotes(Guid sessionId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionNotesQuery { SessionId = sessionId }, ct);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> CreateNote(Guid sessionId, [FromBody] CreateSessionNoteCommand command, CancellationToken ct)
    {
        command.SessionId = sessionId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPut("{noteId}")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> UpdateNote(Guid sessionId, Guid noteId, [FromBody] UpdateSessionNoteCommand command, CancellationToken ct)
    {
        command.NoteId = noteId;
        var result = await _mediator.Send(command, ct);
        return HandleResult(result);
    }
}
