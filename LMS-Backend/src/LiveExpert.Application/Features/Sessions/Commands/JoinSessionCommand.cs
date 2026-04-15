using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Sessions.Commands;

public class JoinSessionCommand : IRequest<Result<JoinSessionResponse>>
{
    public Guid SessionId { get; set; }
}

public class JoinSessionResponse
{
    public string MeetUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Guid SessionId { get; set; }
}

public class JoinSessionCommandHandler : IRequestHandler<JoinSessionCommand, Result<JoinSessionResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<SessionMeetLink> _meetLinkRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;
    private readonly ICalendarConnectionService _calendarConnectionService;
    private readonly IUnitOfWork _unitOfWork;

    public JoinSessionCommandHandler(
        IRepository<User> userRepository,
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<SessionMeetLink> meetLinkRepository,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService,
        ICalendarConnectionService calendarConnectionService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _meetLinkRepository = meetLinkRepository;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
        _calendarConnectionService = calendarConnectionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<JoinSessionResponse>> Handle(JoinSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<JoinSessionResponse>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null || user.Role != UserRole.Student)
        {
            return Result<JoinSessionResponse>.FailureResult("FORBIDDEN", "Only students can join sessions");
        }

        // Students do not need their own calendar connected to join a meeting provided by the tutor

        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<JoinSessionResponse>.FailureResult("NOT_FOUND", "Session not found");
        }

        // Check if session is live
        if (session.Status != SessionStatus.Live && session.Status != SessionStatus.InProgress)
        {
            return Result<JoinSessionResponse>.FailureResult("SESSION_NOT_ACTIVE", 
                "The tutor has not yet started the meeting.");
        }

        // Check if student has booked this session
        var booking = await _bookingRepository.FirstOrDefaultAsync(
            b => b.SessionId == request.SessionId && b.StudentId == userId.Value, cancellationToken);
        if (booking == null)
        {
            return Result<JoinSessionResponse>.FailureResult("NOT_BOOKED", 
                "You must book this session before joining");
        }

        if (booking.BookingStatus != BookingStatus.Confirmed)
        {
            return Result<JoinSessionResponse>.FailureResult("NOT_CONFIRMED",
                "Your booking is not confirmed. Please ensure your payment was successful.");
        }

        // Get Meet link via direct repository query (navigation property projection is unreliable with SQLite)
        var meetLink = await _meetLinkRepository.FirstOrDefaultAsync(
            ml => ml.SessionId == request.SessionId && ml.IsActive, cancellationToken);

        if (meetLink == null)
        {
            return Result<JoinSessionResponse>.FailureResult("NO_MEET_LINK",
                "The meeting link is not available yet. Please wait for the tutor to start the session.");
        }

        // Decrypt Meet URL
        var decryptedUrl = _encryptionService.Decrypt(meetLink.MeetUrl);

        // Update booking join time
        booking.JoinedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<JoinSessionResponse>.SuccessResult(new JoinSessionResponse
        {
            MeetUrl = decryptedUrl,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Temporary access
            SessionId = session.Id
        });
    }
}
