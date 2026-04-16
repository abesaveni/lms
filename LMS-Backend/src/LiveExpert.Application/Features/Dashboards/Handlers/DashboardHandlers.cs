using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Dashboards.Queries;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Dashboards.Handlers;

// Student Dashboard Handler
public class GetStudentDashboardQueryHandler : IRequestHandler<GetStudentDashboardQuery, Result<StudentDashboardDto>>
{
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<BonusPoint> _bonusPointRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentDashboardQueryHandler(
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        IRepository<Subject> subjectRepository,
        IRepository<BonusPoint> bonusPointRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _subjectRepository = subjectRepository;
        _bonusPointRepository = bonusPointRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StudentDashboardDto>> Handle(GetStudentDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentDashboardDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var bookings = await _bookingRepository.FindAsync(b => b.StudentId == userId.Value, cancellationToken);
        var bonusPoints = await _bonusPointRepository.FindAsync(bp => bp.UserId == userId.Value, cancellationToken);

        // Dynamically update status for sessions that have finished
        var now = DateTime.UtcNow;
        foreach (var booking in bookings)
        {
            var session = await _sessionRepository.GetByIdAsync(booking.SessionId, cancellationToken);
            if (session != null && (session.Status == SessionStatus.Scheduled || session.Status == SessionStatus.Live || session.Status == SessionStatus.InProgress) 
                && session.ScheduledAt.AddMinutes(session.Duration) < now)
            {
                session.Status = SessionStatus.Completed;
                session.CompletedAt = now;
                session.UpdatedAt = now;
                await _sessionRepository.UpdateAsync(session, cancellationToken);

                if (booking.BookingStatus == BookingStatus.Confirmed || booking.BookingStatus == BookingStatus.Pending)
                {
                    booking.BookingStatus = booking.AttendanceMarked ? BookingStatus.Completed : BookingStatus.NoShow;
                    booking.UpdatedAt = now;
                    await _bookingRepository.UpdateAsync(booking, cancellationToken);
                }
            }
        }

        // Re-fetch bookings after updates to ensure consistency
        bookings = await _bookingRepository.FindAsync(b => b.StudentId == userId.Value, cancellationToken);

        var completedBookings = bookings.Where(b => b.BookingStatus == BookingStatus.Completed || b.AttendanceMarked).ToList();
        
        var upcomingSessions = new List<UpcomingSessionDto>();
        // Only include bookings where the session is in the future
        foreach (var booking in bookings.Where(b => b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Pending))
        {
            var session = await _sessionRepository.GetByIdAsync(booking.SessionId, cancellationToken);
            if (session != null && session.ScheduledAt.AddMinutes(session.Duration) > now && session.Status != SessionStatus.Completed && session.Status != SessionStatus.Cancelled)
            {
                var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
                upcomingSessions.Add(new UpcomingSessionDto
                {
                    SessionId = session.Id,
                    Title = session.Title,
                    TutorName = tutor?.Username ?? "Unknown",
                    ScheduledAt = session.ScheduledAt,
                    MeetingLink = session.MeetingLink ?? "",
                    Duration = session.Duration,
                    Subject = (await _subjectRepository.GetByIdAsync(session.SubjectId, cancellationToken))?.Name ?? "General",
                    BookingStatus = booking.BookingStatus.ToString()
                });
            }
        }

        var activities = new List<RecentActivityDto>();
        foreach (var bp in bonusPoints)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "Bonus Earned",
                Description = $"Earned {bp.Points} pts: {bp.Reason}",
                Timestamp = bp.CreatedAt
            });
        }
        foreach (var booking in bookings)
        {
            var session = await _sessionRepository.GetByIdAsync(booking.SessionId, cancellationToken);
            activities.Add(new RecentActivityDto
            {
                Type = "Session Booked",
                Description = $"Booked {session?.Title ?? "a session"}",
                Timestamp = booking.CreatedAt
            });
        }

        var dto = new StudentDashboardDto
        {
            TotalBookings = bookings.Count(),
            CompletedSessions = completedBookings.Count,
            UpcomingSessionsCount = upcomingSessions.Count,
            TotalBonusPoints = bonusPoints.Sum(bp => bp.Points),
            UpcomingSessions = upcomingSessions.OrderBy(s => s.ScheduledAt).Take(5).ToList(),
            RecentActivity = activities.OrderByDescending(a => a.Timestamp).Take(5).ToList()
        };

        return Result<StudentDashboardDto>.SuccessResult(dto);
    }
}

// Student Statistics Handler
public class GetStudentStatsQueryHandler : IRequestHandler<GetStudentStatsQuery, Result<StudentStatsDto>>
{
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentStatsQueryHandler(
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        ICurrentUserService currentUserService)
    {
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StudentStatsDto>> Handle(GetStudentStatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentStatsDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var bookings = await _bookingRepository.FindAsync(b => b.StudentId == userId.Value, cancellationToken);
        var activeBookings = bookings.Where(b => b.BookingStatus != BookingStatus.Cancelled && b.BookingStatus != BookingStatus.NoShow).ToList();
        var completedBookings = bookings.Where(b => b.BookingStatus == BookingStatus.Completed || b.AttendanceMarked).ToList();
        
        decimal totalHours = 0;
        foreach (var booking in completedBookings)
        {
            var session = await _sessionRepository.GetByIdAsync(booking.SessionId, cancellationToken);
            if (session != null)
            {
                totalHours += (decimal)session.Duration / 60m;
            }
        }

        var dto = new StudentStatsDto
        {
            TotalSessionsAttended = activeBookings.Count(),
            TotalHoursLearned = (double)Math.Round(totalHours, 1),
            TotalAmountSpent = completedBookings.Sum(b => b.TotalAmount),
            SessionsBySubject = new Dictionary<string, int>(),
            MonthlyActivity = new List<MonthlyActivityDto>()
        };

        return Result<StudentStatsDto>.SuccessResult(dto);
    }
}

// Tutor Dashboard Handler
public class GetTutorDashboardQueryHandler : IRequestHandler<GetTutorDashboardQuery, Result<TutorDashboardDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<TutorEarning> _tutorEarningRepository;
    private readonly IRepository<TutorProfile> _tutorProfileRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorFollower> _followerRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTutorDashboardQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<TutorEarning> tutorEarningRepository,
        IRepository<TutorProfile> tutorProfileRepository,
        IRepository<User> userRepository,
        IRepository<TutorFollower> followerRepository,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _tutorEarningRepository = tutorEarningRepository;
        _tutorProfileRepository = tutorProfileRepository;
        _userRepository = userRepository;
        _followerRepository = followerRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TutorDashboardDto>> Handle(GetTutorDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<TutorDashboardDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var sessions = await _sessionRepository.FindAsync(s => s.TutorId == userId.Value, cancellationToken);
        var earnings = await _tutorEarningRepository.FindAsync(e => e.TutorId == userId.Value, cancellationToken);
        var tutorProfile = await _tutorProfileRepository.FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

        // Dynamically update status for sessions that have finished
        var now = DateTime.UtcNow;
        foreach (var session in sessions)
        {
            if ((session.Status == SessionStatus.Scheduled || session.Status == SessionStatus.Live || session.Status == SessionStatus.InProgress) 
                && session.ScheduledAt.AddMinutes(session.Duration) < now)
            {
                session.Status = SessionStatus.Completed;
                session.CompletedAt = now;
                session.UpdatedAt = now;
                await _sessionRepository.UpdateAsync(session, cancellationToken);

                // Update associated bookings if any
                var associatedBookings = await _bookingRepository.FindAsync(b => b.SessionId == session.Id, cancellationToken);
                foreach (var booking in associatedBookings)
                {
                    if (booking.BookingStatus == BookingStatus.Confirmed || booking.BookingStatus == BookingStatus.Pending)
                    {
                        booking.BookingStatus = booking.AttendanceMarked ? BookingStatus.Completed : BookingStatus.NoShow;
                        booking.UpdatedAt = now;
                        await _bookingRepository.UpdateAsync(booking, cancellationToken);
                    }
                }
            }
        }

        // Re-fetch sessions after updates
        sessions = await _sessionRepository.FindAsync(s => s.TutorId == userId.Value, cancellationToken);

        var completedSessions = sessions.Where(s => s.Status == SessionStatus.Completed).ToList();
        var upcomingSessions = sessions.Where(s => (s.Status == SessionStatus.Scheduled || s.Status == SessionStatus.Live || s.Status == SessionStatus.InProgress) 
                                                && s.ScheduledAt.AddMinutes(s.Duration) > now)
            .OrderBy(s => s.ScheduledAt)
            .Take(5)
            .ToList();

        var upcomingSessionDtos = new List<UpcomingSessionDto>();
        foreach (var session in upcomingSessions)
        {
            upcomingSessionDtos.Add(new UpcomingSessionDto
            {
                SessionId = session.Id,
                Title = session.Title,
                TutorName = "", // Not needed for tutor's own dashboard
                ScheduledAt = session.ScheduledAt,
                MeetingLink = session.MeetingLink ?? ""
            });
        }

        var recentBookings = new List<RecentBookingDto>();
        foreach (var session in sessions.OrderByDescending(s => s.CreatedAt).Take(5))
        {
            var bookings = await _bookingRepository.FindAsync(b => b.SessionId == session.Id, cancellationToken);
            foreach (var booking in bookings.Take(1))
            {
                var student = await _userRepository.GetByIdAsync(booking.StudentId, cancellationToken);
                recentBookings.Add(new RecentBookingDto
                {
                    BookingId = booking.Id,
                    StudentName = student?.Username ?? "Unknown",
                    SessionTitle = session.Title,
                    BookedAt = booking.CreatedAt,
                    ScheduledAt = session.ScheduledAt
                });
            }
        }

        var dto = new TutorDashboardDto
        {
            TotalSessions = sessions.Count(),
            CompletedSessions = completedSessions.Count,
            UpcomingSessionsCount = upcomingSessions.Count,
            TotalEarnings = earnings.Sum(e => e.NetAmount),
            AvailableBalance = earnings.Where(e => e.Status == EarningStatus.Available).Sum(e => e.NetAmount),
            TotalStudents = await (async () => {
                int fromBookings = 0;
                if (sessions.Any())
                {
                    try { fromBookings = (await _bookingRepository.FindAsync(b => sessions.Select(s => s.Id).Contains(b.SessionId) && b.BookingStatus != BookingStatus.Cancelled, cancellationToken)).Select(b => b.StudentId).Distinct().Count(); } catch { }
                }
                int fromFollowers = 0;
                try { fromFollowers = (await _followerRepository.FindAsync(f => f.TutorId == userId.Value, cancellationToken)).Count(); } catch { }
                return Math.Max(fromBookings, fromFollowers);
            })(),
            AverageRating = tutorProfile?.AverageRating ?? 0,
            TotalReviews = tutorProfile?.TotalReviews ?? 0,
            UpcomingSessions = upcomingSessionDtos,
            RecentBookings = recentBookings
        };

        return Result<TutorDashboardDto>.SuccessResult(dto);
    }
}

// Tutor Statistics Handler
public class GetTutorStatsQueryHandler : IRequestHandler<GetTutorStatsQuery, Result<TutorStatsDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<TutorEarning> _tutorEarningRepository;
    private readonly IRepository<TutorProfile> _tutorProfileRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTutorStatsQueryHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<TutorEarning> tutorEarningRepository,
        IRepository<TutorProfile> tutorProfileRepository,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _tutorEarningRepository = tutorEarningRepository;
        _tutorProfileRepository = tutorProfileRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TutorStatsDto>> Handle(GetTutorStatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<TutorStatsDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var sessions = await _sessionRepository.FindAsync(s => s.TutorId == userId.Value, cancellationToken);
        var completedSessions = sessions.Where(s => s.Status == SessionStatus.Completed).ToList();
        var earnings = await _tutorEarningRepository.FindAsync(e => e.TutorId == userId.Value, cancellationToken);
        var tutorProfile = await _tutorProfileRepository.FirstOrDefaultAsync(t => t.UserId == userId.Value, cancellationToken);

        var dto = new TutorStatsDto
        {
            TotalSessionsConducted = completedSessions.Count,
            TotalHoursTaught = completedSessions.Count * 1, // Assuming 1 hour per session
            TotalEarnings = earnings.Sum(e => e.NetAmount),
            TotalWithdrawn = earnings.Where(e => e.Status == EarningStatus.Paid).Sum(e => e.NetAmount),
            UniqueStudents = sessions.Any()
                ? (await _bookingRepository.FindAsync(
                    b => sessions.Select(s => s.Id).Contains(b.SessionId) && b.BookingStatus != BookingStatus.Cancelled,
                    cancellationToken)).Select(b => b.StudentId).Distinct().Count()
                : 0,
            AverageRating = (double)(tutorProfile?.AverageRating ?? 0),
            TotalReviews = tutorProfile?.TotalReviews ?? 0,
            SessionsBySubject = new Dictionary<string, int>(),
            MonthlyEarnings = new List<MonthlyEarningsDto>()
        };

        return Result<TutorStatsDto>.SuccessResult(dto);
    }
}
