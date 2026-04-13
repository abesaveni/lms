using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Sessions.Queries;

// Get My Bookings Query (Student)
public class GetMyBookingsQuery : IRequest<Result<List<MyBookingDto>>>
{
}

public class MyBookingDto
{
    public Guid BookingId { get; set; }
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public string TutorName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AttendanceMarked { get; set; }
    public string MeetingLink { get; set; } = string.Empty;
}

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, Result<List<MyBookingDto>>>
{
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyBookingsQueryHandler(
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MyBookingDto>>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<MyBookingDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var bookings = await _bookingRepository.FindAsync(b => b.StudentId == userId.Value, cancellationToken);

        var bookingDtos = new List<MyBookingDto>();
        foreach (var booking in bookings)
        {
            var session = await _sessionRepository.GetByIdAsync(booking.SessionId, cancellationToken);
            if (session != null)
            {
                var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
                bookingDtos.Add(new MyBookingDto
                {
                    BookingId = booking.Id,
                    SessionId = session.Id,
                    SessionTitle = session.Title,
                    TutorName = tutor?.Username ?? "Unknown",
                    ScheduledAt = session.ScheduledAt,
                    Status = session.Status.ToString(),
                    AttendanceMarked = booking.AttendanceMarked,
                    MeetingLink = session.MeetingLink ?? ""
                });
            }
        }

        return Result<List<MyBookingDto>>.SuccessResult(bookingDtos);
    }
}

// Get My Sessions Query (Tutor)
public class GetMySessionsQuery : IRequest<Result<List<MySessionDto>>>
{
}

public class MySessionDto
{
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BookedSeats { get; set; }
    public int MaxParticipants { get; set; }
    public decimal Price { get; set; }
    public string MeetingLink { get; set; } = string.Empty;
}

public class GetMySessionsQueryHandler : IRequestHandler<GetMySessionsQuery, Result<List<MySessionDto>>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMySessionsQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MySessionDto>>> Handle(GetMySessionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<MySessionDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var sessions = await _sessionRepository.FindAsync(s => s.TutorId == userId.Value, cancellationToken);

        var sessionDtos = new List<MySessionDto>();
        foreach (var session in sessions)
        {
            var bookings = await _bookingRepository.FindAsync(b => b.SessionId == session.Id, cancellationToken);
            
            sessionDtos.Add(new MySessionDto
            {
                SessionId = session.Id,
                Title = session.Title,
                ScheduledAt = session.ScheduledAt,
                Status = session.Status.ToString(),
                BookedSeats = bookings.Count(),
                MaxParticipants = 10, // Default value
                Price = 0, // Default value
                MeetingLink = session.MeetingLink ?? ""
            });
        }

        return Result<List<MySessionDto>>.SuccessResult(sessionDtos);
    }
}
