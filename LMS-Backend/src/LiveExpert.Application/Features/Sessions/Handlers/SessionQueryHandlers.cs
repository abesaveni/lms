using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Sessions.Commands;
using LiveExpert.Application.Features.Sessions.Queries;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.Application.Features.Sessions.Handlers;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, Result<PaginatedResult<SessionDto>>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;

    public GetSessionsQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<User> userRepository,
        IRepository<Subject> subjectRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<Review> reviewRepository,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _subjectRepository = subjectRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedResult<SessionDto>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Session> query = _sessionRepository.GetQueryable()
            .Include(s => s.MeetLink);
        var allSessions = await query.ToListAsync(cancellationToken);

        // Dynamically update status for sessions that have finished
        var now = DateTime.UtcNow;
        var sessionsToUpdate = allSessions
            .Where(s => (s.Status == SessionStatus.Scheduled || s.Status == SessionStatus.Live || s.Status == SessionStatus.InProgress) 
                        && s.ScheduledAt.AddMinutes(s.Duration) < now)
            .ToList();

        if (sessionsToUpdate.Any())
        {
            foreach (var s in sessionsToUpdate)
            {
                s.Status = SessionStatus.Completed;
                s.CompletedAt = now;
                s.UpdatedAt = now;
                await _sessionRepository.UpdateAsync(s, cancellationToken);

                // Update related bookings
                var bookingsToUpdate = await _bookingRepository.FindAsync(b => b.SessionId == s.Id, cancellationToken);
                foreach (var b in bookingsToUpdate)
                {
                    if (b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Pending)
                    {
                        b.BookingStatus = b.AttendanceMarked ? BookingStatus.Completed : BookingStatus.NoShow;
                        b.UpdatedAt = now;
                        await _bookingRepository.UpdateAsync(b, cancellationToken);
                    }
                }
            }
            // Persist the status updates
            try { await _unitOfWork.SaveChangesAsync(cancellationToken); } catch { /* non-critical — updates visible in this request via tracker */ }

            // Re-query after updates
            query = _sessionRepository.GetQueryable()
                .Include(s => s.MeetLink);
        }

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        if (request.TutorId.HasValue)
        {
            query = query.Where(s => s.TutorId == request.TutorId.Value);
        }

        if (request.StudentId.HasValue)
        {
            var studentBookings = await _bookingRepository.FindAsync(
                b => b.StudentId == request.StudentId.Value, cancellationToken);
            var sessionIds = studentBookings.Select(b => b.SessionId).ToList();
            query = query.Where(s => sessionIds.Contains(s.Id));
        }

        if (request.Upcoming == true)
        {
            query = query.Where(s => s.ScheduledAt > now && s.Status != SessionStatus.Cancelled && s.Status != SessionStatus.Completed);
        }

        if (request.Past == true)
        {
            query = query.Where(s => s.ScheduledAt < now);
        }

        // Get total count
        var totalRecords = query.Count();

        // Apply pagination
        var sessions = query
            .OrderBy(s => s.ScheduledAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var sessionDtos = new List<SessionDto>();
        foreach (var session in sessions)
        {
            var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
            var subject = await _subjectRepository.GetByIdAsync(session.SubjectId, cancellationToken);

            bool isBooked = false;
            if (_currentUserService.UserId.HasValue)
            {
                isBooked = await _bookingRepository.AnyAsync(
                    b => b.SessionId == session.Id && b.StudentId == _currentUserService.UserId.Value,
                    cancellationToken);
            }

            string statusValue = session.Status.ToString();
            var targetStudentId = request.StudentId ?? _currentUserService.UserId;

            if (targetStudentId.HasValue)
            {
                var booking = await _bookingRepository.FirstOrDefaultAsync(
                    b => b.SessionId == session.Id && b.StudentId == targetStudentId.Value,
                    cancellationToken);
                
                if (booking != null)
                {
                    statusValue = booking.BookingStatus.ToString();
                }
            }

            bool isReviewed = false;
            if (_currentUserService.UserId.HasValue)
            {
                isReviewed = await _reviewRepository.AnyAsync(
                    r => r.SessionId == session.Id && r.StudentId == _currentUserService.UserId.Value,
                    cancellationToken);
            }

            sessionDtos.Add(new SessionDto
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
                Status = statusValue,
                MeetingLink = session.MeetLink != null ? _encryptionService.Decrypt(session.MeetLink.MeetUrl) : session.MeetingLink,
                IsBooked = isBooked,
                IsReviewed = isReviewed,
                CreatedAt = session.CreatedAt
            });
        }

        var result = new PaginatedResult<SessionDto>
        {
            Items = sessionDtos,
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            }
        };

        return Result<PaginatedResult<SessionDto>>.SuccessResult(result);
    }
}

public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, Result<SessionDetailDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IEncryptionService _encryptionService;

    public GetSessionByIdQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<User> userRepository,
        IRepository<Subject> subjectRepository,
        IRepository<SessionBooking> bookingRepository,
        IEncryptionService encryptionService)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _subjectRepository = subjectRepository;
        _bookingRepository = bookingRepository;
        _encryptionService = encryptionService;
    }

    public async Task<Result<SessionDetailDto>> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetQueryable()
            .Include(s => s.MeetLink)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);
            
        if (session == null)
        {
            return Result<SessionDetailDto>.FailureResult("NOT_FOUND", "Session not found");
        }

        var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
        var subject = await _subjectRepository.GetByIdAsync(session.SubjectId, cancellationToken);
        var bookings = await _bookingRepository.FindAsync(
            b => b.SessionId == request.SessionId, cancellationToken);

        var bookingDetails = new List<BookingDetailDto>();
        foreach (var booking in bookings)
        {
            var student = await _userRepository.GetByIdAsync(booking.StudentId, cancellationToken);
            bookingDetails.Add(new BookingDetailDto
            {
                Id = booking.Id,
                StudentId = booking.StudentId,
                StudentName = student?.Username ?? "",
                StudentImage = student?.ProfileImageUrl ?? "",
                Status = booking.BookingStatus,
                AttendanceMarked = booking.AttendanceMarked,
                JoinedAt = booking.JoinedAt,
                LeftAt = booking.LeftAt
            });
        }

        var sessionDetail = new SessionDetailDto
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
            MeetingLink = session.MeetLink != null ? _encryptionService.Decrypt(session.MeetLink.MeetUrl) : session.MeetingLink,
            RecordingUrl = session.RecordingUrl,
            IsRecorded = session.IsRecorded,
            CompletedAt = session.CompletedAt,
            CreatedAt = session.CreatedAt,
            Bookings = bookingDetails
        };

        return Result<SessionDetailDto>.SuccessResult(sessionDetail);
    }
}

public class GetSessionMeetingLinkQueryHandler : IRequestHandler<GetSessionMeetingLinkQuery, Result<MeetingLinkDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<SessionMeetLink> _meetLinkRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;

    public GetSessionMeetingLinkQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<SessionMeetLink> meetLinkRepository,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _meetLinkRepository = meetLinkRepository;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
    }

    public async Task<Result<MeetingLinkDto>> Handle(GetSessionMeetingLinkQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<MeetingLinkDto>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Result<MeetingLinkDto>.FailureResult("NOT_FOUND", "Session not found");
        }

        // Check if user is tutor or has booked the session
        bool hasAccess = session.TutorId == userId.Value;
        if (!hasAccess)
        {
            hasAccess = await _bookingRepository.AnyAsync(
                b => b.SessionId == request.SessionId && b.StudentId == userId.Value && 
                (b.BookingStatus == Domain.Enums.BookingStatus.Confirmed || b.BookingStatus == Domain.Enums.BookingStatus.Pending),
                cancellationToken);
        }

        if (!hasAccess)
        {
            return Result<MeetingLinkDto>.FailureResult("FORBIDDEN", "You don't have access to this session");
        }

        // Get Meet link from the SessionMeetLink repository
        var meetLink = await _meetLinkRepository.FirstOrDefaultAsync(
            ml => ml.SessionId == request.SessionId && ml.IsActive, cancellationToken);
            
        string? decryptedUrl = null;
        
        if (meetLink == null)
        {
            // FALLBACK: If no link exists in SessionMeetLink, check the legacy field
            if (!string.IsNullOrEmpty(session.MeetingLink))
            {
                decryptedUrl = session.MeetingLink;
            }
            else
            {
                return Result<MeetingLinkDto>.FailureResult("NO_MEET_LINK", "Meeting link not available. Has the session started?");
            }
        }
        else
        {
            // Decrypt the URL from SessionMeetLink
            decryptedUrl = _encryptionService.Decrypt(meetLink.MeetUrl);
        }

        return Result<MeetingLinkDto>.SuccessResult(new MeetingLinkDto
        {
            MeetingLink = decryptedUrl,
            ScheduledAt = session.ScheduledAt,
            Duration = session.Duration
        });
    }
}
public class GetSessionPricingQueryHandler : IRequestHandler<GetSessionPricingQuery, Result<SessionPricingDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISystemSettingsService _settingsService;

    public GetSessionPricingQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        ICurrentUserService currentUserService,
        ISystemSettingsService settingsService)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
        _settingsService = settingsService;
    }

    public async Task<Result<SessionPricingDto>> Handle(GetSessionPricingQuery request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Result<SessionPricingDto>.FailureResult("NOT_FOUND", "Session not found");
        }

        int? hoursBooked = null;
        if (session.PricingType == SessionPricingType.Hourly)
        {
            if (!request.Hours.HasValue || request.Hours.Value <= 0)
            {
                return Result<SessionPricingDto>.FailureResult("INVALID_HOURS", "Hours are required for hourly sessions");
            }
            hoursBooked = request.Hours.Value;
        }

        var baseAmount = session.PricingType == SessionPricingType.Hourly
            ? session.BasePrice * (hoursBooked ?? 0)
            : session.BasePrice;

        var platformFee = 0m;
        var userId = _currentUserService.UserId;
        bool isFirstBooking = true;
        
        if (userId.HasValue)
        {
            var bookingCount = await _bookingRepository.CountAsync(b => b.StudentId == userId.Value);
            isFirstBooking = bookingCount == 0;
        }

        var platformFeeEnabled = await _settingsService.IsPlatformFeeEnabledAsync();
        if (platformFeeEnabled && isFirstBooking)
        {
            var feeType = await _settingsService.GetPlatformFeeTypeAsync();
            switch (feeType)
            {
                case PlatformFeeType.Fixed:
                    platformFee = await _settingsService.GetPlatformFeeFixedAsync();
                    break;
                case PlatformFeeType.PerHour:
                    platformFee = await _settingsService.GetPlatformFeePerHourAsync() * (hoursBooked ?? 1);
                    break;
                case PlatformFeeType.Percentage:
                    var percentage = await _settingsService.GetPlatformFeePercentageAsync();
                    platformFee = Math.Round(baseAmount * (percentage / 100m), 2);
                    break;
            }
        }

        var totalAmount = baseAmount + platformFee;

        return Result<SessionPricingDto>.SuccessResult(new SessionPricingDto
        {
            Hours = hoursBooked,
            BaseAmount = baseAmount,
            PlatformFee = platformFee,
            TotalAmount = totalAmount
        });
    }
}
