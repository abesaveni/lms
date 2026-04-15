using LiveExpert.Application.Common;
using MediatR;

namespace LiveExpert.Application.Features.Sessions.Commands;

// Feature 2: Session Notes

public class CreateSessionNoteCommand : IRequest<Result<SessionNoteDto>>
{
    public Guid SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsVisibleToStudent { get; set; } = true;
}

public class UpdateSessionNoteCommand : IRequest<Result<SessionNoteDto>>
{
    public Guid NoteId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsVisibleToStudent { get; set; } = true;
}

public class GetSessionNotesQuery : IRequest<Result<List<SessionNoteDto>>>
{
    public Guid SessionId { get; set; }
}

public class SessionNoteDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsVisibleToStudent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
