using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Sessions.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.Sessions.Handlers;

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, Result<SessionDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarConnectionService _calendarConnectionService;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IEncryptionService _encryptionService;
    private readonly IRepository<SessionMeetLink> _meetLinkRepository;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly ILogger<CreateSessionCommandHandler> _logger;

    public CreateSessionCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<Subject> subjectRepository,
        IRepository<User> userRepository,
        IRepository<SessionMeetLink> meetLinkRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ICalendarConnectionService calendarConnectionService,
        IGoogleCalendarService googleCalendarService,
        IEncryptionService encryptionService,
        INotificationDispatcher notificationDispatcher,
        ILogger<CreateSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _tutorRepository = tutorRepository;
        _subjectRepository = subjectRepository;
        _userRepository = userRepository;
        _meetLinkRepository = meetLinkRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _calendarConnectionService = calendarConnectionService;
        _googleCalendarService = googleCalendarService;
        _encryptionService = encryptionService;
        _notificationDispatcher = notificationDispatcher;
        _logger = logger;
    }

    public async Task<Result<SessionDto>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<SessionDto>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        // Verify user is a tutor
        var tutorProfile = await _tutorRepository.FirstOrDefaultAsync(
            tp => tp.UserId == userId.Value, cancellationToken);

        if (tutorProfile == null)
        {
            return Result<SessionDto>.FailureResult("FORBIDDEN", "Only tutors can create sessions");
        }

        // MANDATORY: Check if tutor is verified - BYPASSED FOR TESTING
        /*
        if (tutorProfile.VerificationStatus != VerificationStatus.Approved)
        {
            return Result<SessionDto>.FailureResult("NOT_VERIFIED", 
                "Your tutor account must be verified before creating sessions. Please wait for admin approval.");
        }
        */

        // MANDATORY: Check Google Calendar connection - BYPASSED FOR TESTING
        /*
        var isCalendarConnected = await _calendarConnectionService.IsCalendarConnectedAsync(userId.Value, cancellationToken);
        if (!isCalendarConnected)
        {
            return Result<SessionDto>.FailureResult("CALENDAR_NOT_CONNECTED", 
                "Google Calendar connection is required to create sessions. Please connect your calendar first.");
        }
        */

        // Verify subject exists
        var subject = await _subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);
        if (subject == null)
        {
            return Result<SessionDto>.FailureResult("NOT_FOUND", "Subject not found");
        }

        /*
        // Get tutor's access token for Google Calendar - BYPASSED FOR TESTING
        var accessToken = await _calendarConnectionService.GetValidAccessTokenAsync(userId.Value, cancellationToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Result<SessionDto>.FailureResult("CALENDAR_TOKEN_ERROR", 
                "Failed to get Google Calendar access. Please reconnect your calendar.");
        }
        */

        // Create Google Calendar event with Meet link
        var endTime = request.ScheduledAt.AddMinutes(request.Duration);
        // Default to a real Jitsi meeting (works without Google Calendar OAuth)
        var jitsiCode = Guid.NewGuid().ToString("N").Substring(0, 12).ToLower();
        string meetUrl = $"https://meet.jit.si/LiveExpert-{jitsiCode}";
        string? calendarEventId = null;

        try
        {
            // Try to create a real Google Meet link if calendar is configured
            var realMeetUrl = await _googleCalendarService.CreateMeetingLinkAsync(
                request.Title,
                request.ScheduledAt,
                endTime,
                cancellationToken);

            if (!string.IsNullOrEmpty(realMeetUrl)) meetUrl = realMeetUrl;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Google Calendar not configured — using Jitsi fallback for meeting link.");
        }

        // Encrypt Meet URL
        var encryptedMeetUrl = _encryptionService.Encrypt(meetUrl);

        // Create session
        var session = new Session
        {
            Id = Guid.NewGuid(),
            TutorId = userId.Value,
            SessionType = request.SessionType,
            PricingType = request.PricingType,
            Title = request.Title,
            Description = request.Description,
            SubjectId = request.SubjectId,
            ScheduledAt = request.ScheduledAt,
            Duration = request.Duration,
            BasePrice = request.BasePrice,
            MaxStudents = request.SessionType == SessionType.OneOnOne ? 1 : request.MaxStudents,
            CurrentStudents = 0,
            Status = SessionStatus.Scheduled,
            GoogleCalendarEventId = calendarEventId,
            IsRecorded = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create SessionMeetLink entry
        var meetLink = new SessionMeetLink
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            MeetUrl = encryptedMeetUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _meetLinkRepository.AddAsync(meetLink, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tutor = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (tutor != null)
        {
            var tutorName = $"{tutor.FirstName} {tutor.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(tutorName))
            {
                tutorName = tutor.Username;
            }

            var sessionTime = session.ScheduledAt.ToString("f");
            var emailBody = EmailTemplates.SessionScheduledEmail(tutorName, session.Title, sessionTime, "/tutor/sessions", "Various students", "Tutor");

            try
            {
                await _notificationDispatcher.SendAsync(new NotificationDispatchRequest
                {
                    UserId = tutor.Id,
                    Category = NotificationCategory.SessionBooking,
                    IsTransactional = true,
                    Title = "Session Created",
                    Message = $"Your session \"{session.Title}\" has been created.",
                    ActionUrl = "/tutor/sessions",
                    EmailTo = tutor.Email,
                    EmailSubject = "Session Created Successfully",
                    EmailBody = emailBody,
                    EmailIsHtml = true,
                    WhatsAppTo = tutor.WhatsAppNumber ?? tutor.PhoneNumber,
                    WhatsAppMessage = $"✅ Session Created!\n\nHi {tutorName}, your session *{session.Title}* has been created and is now live.\n\nScheduled: {session.ScheduledAt:dd MMM yyyy, HH:mm}\n\nManage it at LiveExpert.ai/tutor/sessions",
                    SendInApp = true
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send session creation notification — SMTP not configured");
            }
        }

        return Result<SessionDto>.SuccessResult(new SessionDto
        {
            Id = session.Id,
            TutorId = session.TutorId,
            TutorName = tutor?.Username ?? "",
            TutorImage = tutor?.ProfileImageUrl ?? "",
            SessionType = session.SessionType,
            PricingType = session.PricingType,
            Title = session.Title,
            Description = session.Description,
            SubjectName = subject.Name,
            ScheduledAt = session.ScheduledAt,
            Duration = session.Duration,
            BasePrice = session.BasePrice,
            MaxStudents = session.MaxStudents,
            CurrentStudents = session.CurrentStudents,
            Status = session.Status.ToString(),
            MeetingLink = session.MeetingLink,
            IsBooked = false,
            CreatedAt = session.CreatedAt
        });
    }
}

public class UpdateSessionCommandHandler : IRequestHandler<UpdateSessionCommand, Result<SessionDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSessionCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<Subject> subjectRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _subjectRepository = subjectRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SessionDto>> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<SessionDto>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Result<SessionDto>.FailureResult("NOT_FOUND", "Session not found");
        }

        if (session.TutorId != userId.Value)
        {
            return Result<SessionDto>.FailureResult("FORBIDDEN", "You can only update your own sessions");
        }

        // Update session
        session.Title = request.Title;
        session.Description = request.Description;
        session.ScheduledAt = request.ScheduledAt;
        session.Duration = request.Duration;
        session.BasePrice = request.BasePrice;
        session.PricingType = request.PricingType;
        session.UpdatedAt = DateTime.UtcNow;

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var subject = await _subjectRepository.GetByIdAsync(session.SubjectId, cancellationToken);
        var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);

        return Result<SessionDto>.SuccessResult(new SessionDto
        {
            Id = session.Id,
            TutorId = session.TutorId,
            TutorName = tutor?.Username ?? "",
            TutorImage = tutor?.ProfileImageUrl ?? "",
            SessionType = session.SessionType,
            PricingType = session.PricingType,
            Title = session.Title,
            Description = session.Description,
            SubjectName = subject?.Name ?? "",
            ScheduledAt = session.ScheduledAt,
            Duration = session.Duration,
            BasePrice = session.BasePrice,
            MaxStudents = session.MaxStudents,
            CurrentStudents = session.CurrentStudents,
            Status = session.Status.ToString(),
            MeetingLink = session.MeetingLink,
            IsBooked = false,
            CreatedAt = session.CreatedAt
        });
    }
}

public class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, Result>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSessionCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
            {
                return Result.FailureResult("NOT_FOUND", "Session not found");
            }

            if (session.TutorId != userId.Value)
            {
                return Result.FailureResult("FORBIDDEN", "You can only cancel your own sessions");
            }

            // Get all bookings
            var bookings = await _bookingRepository.FindAsync(
                b => b.SessionId == request.SessionId && b.BookingStatus == BookingStatus.Confirmed,
                cancellationToken);

            foreach (var booking in bookings)
            {
                booking.BookingStatus = BookingStatus.Cancelled;
                await _bookingRepository.UpdateAsync(booking, cancellationToken);

                var student = await _userRepository.GetByIdAsync(booking.StudentId, cancellationToken);
                if (student != null)
                {
                    await _notificationService.SendSessionCancelledAsync(
                        student,
                        session.Title,
                        session.ScheduledAt,
                        "the Tutor",
                        cancellationToken);
                }
            }

            // Update session status
            session.Status = SessionStatus.Cancelled;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.SuccessResult("Session cancelled successfully");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public class CompleteSessionCommandHandler : IRequestHandler<CompleteSessionCommand, Result>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<TutorEarning> _tutorEarningRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteSessionCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<TutorEarning> tutorEarningRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _tutorEarningRepository = tutorEarningRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
            {
                return Result.FailureResult("NOT_FOUND", "Session not found");
            }

            if (session.TutorId != userId.Value)
            {
                return Result.FailureResult("FORBIDDEN", "You can only complete your own sessions");
            }

            if (session.Status == SessionStatus.Completed)
            {
                return Result.FailureResult("ALREADY_COMPLETED", "Session is already completed");
            }

            // Update session status
            session.Status = SessionStatus.Completed;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, cancellationToken);

            // Update all bookings to completed
            var bookings = await _bookingRepository.FindAsync(
                b => b.SessionId == request.SessionId && b.BookingStatus == BookingStatus.Confirmed,
                cancellationToken);

            foreach (var booking in bookings)
            {
                booking.BookingStatus = BookingStatus.Completed;
                await _bookingRepository.UpdateAsync(booking, cancellationToken);

                // Notify student
                var student = await _userRepository.GetByIdAsync(booking.StudentId, cancellationToken);
                if (student != null)
                {
                    await _notificationService.SendNotificationAsync(
                        student.Id,
                        "Session Completed",
                        $"Your session \"{session.Title}\" has been marked as completed. Please leave a review!",
                        NotificationType.SessionCompleted,
                        $"/student/sessions",
                        cancellationToken);
                }
            }

            // Make tutor earnings available
            var earnings = await _tutorEarningRepository.FindAsync(
                e => e.SourceId == request.SessionId && e.Status == EarningStatus.Pending,
                cancellationToken);

            foreach (var earning in earnings)
            {
                earning.Status = EarningStatus.Available;
                earning.AvailableAt = DateTime.UtcNow;
                earning.UpdatedAt = DateTime.UtcNow;
                await _tutorEarningRepository.UpdateAsync(earning, cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.SuccessResult("Session marked as completed");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
