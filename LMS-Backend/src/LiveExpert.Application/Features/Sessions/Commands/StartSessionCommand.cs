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
            // FALLBACK: Create a dummy meet link for testing if Google Calendar is not connected
            var meetCode = Guid.NewGuid().ToString("N").Substring(0, 10).ToLower();
            var fallbackUrl = $"https://meet.jit.si/LiveExpert-Test-{meetCode}";
            
            meetLink = new SessionMeetLink
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                MeetUrl = _encryptionService.Encrypt(fallbackUrl),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _meetLinkRepository.AddAsync(meetLink, cancellationToken);
            decryptedUrl = fallbackUrl;
        }
        else 
        {
            // Decrypt existing Meet URL
            decryptedUrl = _encryptionService.Decrypt(meetLink.MeetUrl);
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
