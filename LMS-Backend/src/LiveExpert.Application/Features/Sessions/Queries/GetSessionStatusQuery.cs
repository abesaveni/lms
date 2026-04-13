using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.Application.Features.Sessions.Queries;

public class GetSessionStatusQuery : IRequest<Result<SessionStatusResponse>>
{
    public Guid SessionId { get; set; }
}

public class SessionStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public bool CanJoin { get; set; }
    public bool TutorStarted { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class GetSessionStatusQueryHandler : IRequestHandler<GetSessionStatusQuery, Result<SessionStatusResponse>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetSessionStatusQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SessionStatusResponse>> Handle(GetSessionStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<SessionStatusResponse>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<SessionStatusResponse>.FailureResult("NOT_FOUND", "Session not found");
        }

        var isTutor = session.TutorId == userId.Value;
        var bookings = await _bookingRepository.FindAsync(b => b.SessionId == request.SessionId, cancellationToken);
        var isStudent = bookings.Any(b => b.StudentId == userId.Value);
        
        // Get Meet link
        var meetLink = await _sessionRepository.GetQueryable()
            .Where(s => s.Id == request.SessionId)
            .Select(s => s.MeetLink)
            .FirstOrDefaultAsync(cancellationToken);

        if (!isTutor && !isStudent)
        {
            return Result<SessionStatusResponse>.FailureResult("FORBIDDEN", "You don't have access to this session");
        }

        var tutorStarted = session.Status == Domain.Enums.SessionStatus.Live || session.Status == Domain.Enums.SessionStatus.InProgress;
        var canJoin = tutorStarted && (isTutor || isStudent);

        return Result<SessionStatusResponse>.SuccessResult(new SessionStatusResponse
        {
            Status = session.Status.ToString(),
            CanJoin = canJoin,
            TutorStarted = tutorStarted,
            StartTime = meetLink?.SessionStartedAt,
            ScheduledAt = session.ScheduledAt
        });
    }
}
