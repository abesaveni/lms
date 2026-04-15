using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Sessions.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.Sessions.Handlers;

// Feature 2: Session Notes

public class CreateSessionNoteCommandHandler : IRequestHandler<CreateSessionNoteCommand, Result<SessionNoteDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionNote> _noteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSessionNoteCommandHandler> _logger;

    public CreateSessionNoteCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionNote> noteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CreateSessionNoteCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _noteRepository = noteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SessionNoteDto>> Handle(CreateSessionNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<SessionNoteDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
                return Result<SessionNoteDto>.FailureResult("NOT_FOUND", "Session not found");

            if (session.TutorId != userId.Value)
                return Result<SessionNoteDto>.FailureResult("FORBIDDEN", "Only the session tutor can add notes");

            var note = new SessionNote
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                TutorId = userId.Value,
                Content = request.Content,
                IsVisibleToStudent = request.IsVisibleToStudent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _noteRepository.AddAsync(note, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<SessionNoteDto>.SuccessResult(new SessionNoteDto
            {
                Id = note.Id,
                SessionId = note.SessionId,
                Content = note.Content,
                IsVisibleToStudent = note.IsVisibleToStudent,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating session note");
            return Result<SessionNoteDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class UpdateSessionNoteCommandHandler : IRequestHandler<UpdateSessionNoteCommand, Result<SessionNoteDto>>
{
    private readonly IRepository<SessionNote> _noteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSessionNoteCommandHandler> _logger;

    public UpdateSessionNoteCommandHandler(
        IRepository<SessionNote> noteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSessionNoteCommandHandler> logger)
    {
        _noteRepository = noteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SessionNoteDto>> Handle(UpdateSessionNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<SessionNoteDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var note = await _noteRepository.GetByIdAsync(request.NoteId, cancellationToken);
            if (note == null)
                return Result<SessionNoteDto>.FailureResult("NOT_FOUND", "Note not found");

            if (note.TutorId != userId.Value)
                return Result<SessionNoteDto>.FailureResult("FORBIDDEN", "Only the note author can update it");

            note.Content = request.Content;
            note.IsVisibleToStudent = request.IsVisibleToStudent;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.UpdateAsync(note, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<SessionNoteDto>.SuccessResult(new SessionNoteDto
            {
                Id = note.Id,
                SessionId = note.SessionId,
                Content = note.Content,
                IsVisibleToStudent = note.IsVisibleToStudent,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error updating session note");
            return Result<SessionNoteDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetSessionNotesQueryHandler : IRequestHandler<GetSessionNotesQuery, Result<List<SessionNoteDto>>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionNote> _noteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetSessionNotesQueryHandler> _logger;

    public GetSessionNotesQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionNote> noteRepository,
        ICurrentUserService currentUserService,
        ILogger<GetSessionNotesQueryHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _noteRepository = noteRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<SessionNoteDto>>> Handle(GetSessionNotesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<SessionNoteDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
                return Result<List<SessionNoteDto>>.FailureResult("NOT_FOUND", "Session not found");

            IEnumerable<SessionNote> notes;
            if (session.TutorId == userId.Value)
            {
                // Tutor sees all notes
                notes = await _noteRepository.FindAsync(n => n.SessionId == request.SessionId, cancellationToken);
            }
            else
            {
                // Student only sees notes marked visible
                notes = await _noteRepository.FindAsync(
                    n => n.SessionId == request.SessionId && n.IsVisibleToStudent, cancellationToken);
            }

            var dtos = notes.OrderByDescending(n => n.CreatedAt).Select(n => new SessionNoteDto
            {
                Id = n.Id,
                SessionId = n.SessionId,
                Content = n.Content,
                IsVisibleToStudent = n.IsVisibleToStudent,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            }).ToList();

            return Result<List<SessionNoteDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session notes");
            return Result<List<SessionNoteDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}
