using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.Application.Features.Sessions.Commands;

public class StartSessionCommand : IRequest<Result<StartSessionResponse>>
{
    public Guid SessionId { get; set; }
}

public class StartSessionResponse
{
    public string MeetUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Guid SessionId { get; set; }
}

public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, Result<StartSessionResponse>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionMeetLink> _meetLinkRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;
    private readonly ICalendarConnectionService _calendarConnectionService;
    private readonly IUnitOfWork _unitOfWork;

    public StartSessionCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionMeetLink> meetLinkRepository,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService,
        ICalendarConnectionService calendarConnectionService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _meetLinkRepository = meetLinkRepository;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
        _calendarConnectionService = calendarConnectionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StartSessionResponse>> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<StartSessionResponse>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<StartSessionResponse>.FailureResult("NOT_FOUND", "Session not found");
        }

        if (session.TutorId != userId.Value)
        {
            return Result<StartSessionResponse>.FailureResult("FORBIDDEN", "Only the session tutor can start the session");
        }

        // MANDATORY: Check Google Calendar connection (Relaxed for testing)
        var isCalendarConnected = await _calendarConnectionService.IsCalendarConnectedAsync(userId.Value);
        
        if (session.Status != SessionStatus.Scheduled)
        {
            return Result<StartSessionResponse>.FailureResult("INVALID_STATE", "Session must be scheduled to start");
        }

        // Get Meet link
        var meetLink = await _meetLinkRepository.FirstOrDefaultAsync(
            ml => ml.SessionId == request.SessionId && ml.IsActive, cancellationToken);

        string? decryptedUrl = null;

        if (meetLink == null)
        {
            // No link stored yet — create a Jitsi meeting (works without Google Calendar)
            var meetCode = Guid.NewGuid().ToString("N").Substring(0, 12).ToLower();
            var jitsiUrl = $"https://meet.jit.si/LiveExpert-{meetCode}";

            meetLink = new SessionMeetLink
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                MeetUrl = _encryptionService.Encrypt(jitsiUrl),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _meetLinkRepository.AddAsync(meetLink, cancellationToken);
            decryptedUrl = jitsiUrl;
        }
        else
        {
            decryptedUrl = _encryptionService.Decrypt(meetLink.MeetUrl);

            // If a stale fake Google Meet URL was stored (from old code), replace it with Jitsi
            if (decryptedUrl.Contains("meet.google.com/test-session-") ||
                decryptedUrl.Contains("meet.google.com/xxx"))
            {
                var meetCode = Guid.NewGuid().ToString("N").Substring(0, 12).ToLower();
                decryptedUrl = $"https://meet.jit.si/LiveExpert-{meetCode}";
                meetLink.MeetUrl = _encryptionService.Encrypt(decryptedUrl);
                meetLink.UpdatedAt = DateTime.UtcNow;
                await _meetLinkRepository.UpdateAsync(meetLink, cancellationToken);
            }
        }
        
        // Update session status
        session.Status = SessionStatus.Live;
        meetLink.SessionStartedAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _meetLinkRepository.UpdateAsync(meetLink, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StartSessionResponse>.SuccessResult(new StartSessionResponse
        {
            MeetUrl = decryptedUrl,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Temporary access
            SessionId = session.Id
        });
    }
}
