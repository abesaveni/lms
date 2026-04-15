using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Students.Queries;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.Students.Handlers;

// Feature 5: Re-book with same tutor
public class GetRecentTutorsQueryHandler : IRequestHandler<GetRecentTutorsQuery, Result<List<RecentTutorDto>>>
{
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetRecentTutorsQueryHandler> _logger;

    public GetRecentTutorsQueryHandler(
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        IRepository<User> userRepository,
        IRepository<Review> reviewRepository,
        ICurrentUserService currentUserService,
        ILogger<GetRecentTutorsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<RecentTutorDto>>> Handle(GetRecentTutorsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<RecentTutorDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            // Get completed bookings for this student
            var completedBookings = (await _bookingRepository.FindAsync(
                b => b.StudentId == userId.Value && b.BookingStatus == BookingStatus.Completed,
                cancellationToken)).ToList();

            if (!completedBookings.Any())
                return Result<List<RecentTutorDto>>.SuccessResult(new List<RecentTutorDto>());

            var sessionIds = completedBookings.Select(b => b.SessionId).Distinct().ToList();

            // Get sessions
            var sessionsQuery = _sessionRepository.GetQueryable()
                .Include(s => s.Subject)
                .Where(s => sessionIds.Contains(s.Id));
            var sessions = await sessionsQuery.ToListAsync(cancellationToken);

            // Group by TutorId
            var tutorGroups = sessions
                .GroupBy(s => s.TutorId)
                .Select(g => new
                {
                    TutorId = g.Key,
                    LastSessionDate = g.Max(s => s.ScheduledAt),
                    SubjectName = g.OrderByDescending(s => s.ScheduledAt).First().Subject?.Name ?? "General"
                })
                .OrderByDescending(g => g.LastSessionDate)
                .Take(10)
                .ToList();

            var tutorIds = tutorGroups.Select(t => t.TutorId).ToList();

            // Get tutors
            var tutors = (await _userRepository.FindAsync(u => tutorIds.Contains(u.Id), cancellationToken))
                .ToDictionary(u => u.Id);

            // Get reviews for average ratings
            var reviews = (await _reviewRepository.FindAsync(
                r => tutorIds.Contains(r.TutorId), cancellationToken)).ToList();

            var result = tutorGroups.Select(g =>
            {
                tutors.TryGetValue(g.TutorId, out var tutor);
                var tutorReviews = reviews.Where(r => r.TutorId == g.TutorId).ToList();
                return new RecentTutorDto
                {
                    TutorId = g.TutorId,
                    TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "Unknown",
                    LastSessionDate = g.LastSessionDate,
                    SubjectName = g.SubjectName,
                    AverageRating = tutorReviews.Any() ? Math.Round(tutorReviews.Average(r => r.Rating), 2) : 0
                };
            }).ToList();

            return Result<List<RecentTutorDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent tutors");
            return Result<List<RecentTutorDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}

// Feature 11: Tutor recommendations
public class GetRecommendedTutorsQueryHandler : IRequestHandler<GetRecommendedTutorsQuery, Result<List<RecommendedTutorDto>>>
{
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorProfileRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetRecommendedTutorsQueryHandler> _logger;

    public GetRecommendedTutorsQueryHandler(
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorProfileRepository,
        IRepository<Review> reviewRepository,
        ICurrentUserService currentUserService,
        ILogger<GetRecommendedTutorsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _tutorProfileRepository = tutorProfileRepository;
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<RecommendedTutorDto>>> Handle(GetRecommendedTutorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var maxResults = Math.Max(1, Math.Min(request.MaxResults, 50));

            // Get verified tutors
            var verifiedProfiles = (await _tutorProfileRepository.FindAsync(
                tp => tp.VerificationStatus == VerificationStatus.Approved && tp.IsVisible,
                cancellationToken)).ToList();

            if (!verifiedProfiles.Any())
                return Result<List<RecommendedTutorDto>>.SuccessResult(new List<RecommendedTutorDto>());

            var verifiedTutorIds = verifiedProfiles.Select(p => p.UserId).ToList();

            // If SubjectId filter provided, get only sessions in that subject
            IEnumerable<Session> relevantSessions;
            if (request.SubjectId.HasValue)
            {
                relevantSessions = await _sessionRepository.GetQueryable()
                    .Include(s => s.Subject)
                    .Where(s => verifiedTutorIds.Contains(s.TutorId) && s.SubjectId == request.SubjectId.Value)
                    .ToListAsync(cancellationToken);
            }
            else if (userId.HasValue)
            {
                // Get student's past subject IDs
                var studentBookings = (await _bookingRepository.FindAsync(
                    b => b.StudentId == userId.Value && b.BookingStatus == BookingStatus.Completed,
                    cancellationToken)).ToList();

                var pastSessionIds = studentBookings.Select(b => b.SessionId).ToList();
                var pastSessions = pastSessionIds.Any()
                    ? await _sessionRepository.GetQueryable()
                        .Where(s => pastSessionIds.Contains(s.Id))
                        .ToListAsync(cancellationToken)
                    : new List<Session>();

                var pastSubjectIds = pastSessions.Select(s => s.SubjectId).Distinct().ToList();

                if (pastSubjectIds.Any())
                {
                    relevantSessions = await _sessionRepository.GetQueryable()
                        .Include(s => s.Subject)
                        .Where(s => verifiedTutorIds.Contains(s.TutorId) && pastSubjectIds.Contains(s.SubjectId))
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    relevantSessions = await _sessionRepository.GetQueryable()
                        .Include(s => s.Subject)
                        .Where(s => verifiedTutorIds.Contains(s.TutorId))
                        .ToListAsync(cancellationToken);
                }
            }
            else
            {
                relevantSessions = await _sessionRepository.GetQueryable()
                    .Include(s => s.Subject)
                    .Where(s => verifiedTutorIds.Contains(s.TutorId))
                    .ToListAsync(cancellationToken);
            }

            var sessionsByTutor = relevantSessions
                .GroupBy(s => s.TutorId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var reviews = (await _reviewRepository.FindAsync(
                r => verifiedTutorIds.Contains(r.TutorId), cancellationToken)).ToList();

            var reviewsByTutor = reviews.GroupBy(r => r.TutorId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var tutors = (await _userRepository.FindAsync(
                u => verifiedTutorIds.Contains(u.Id), cancellationToken))
                .ToDictionary(u => u.Id);

            var profilesByTutor = verifiedProfiles.ToDictionary(p => p.UserId);

            var result = sessionsByTutor.Keys
                .Select(tutorId =>
                {
                    tutors.TryGetValue(tutorId, out var tutor);
                    profilesByTutor.TryGetValue(tutorId, out var profile);
                    reviewsByTutor.TryGetValue(tutorId, out var tutorReviews);
                    var tutorSessions = sessionsByTutor[tutorId];
                    var subjectNames = tutorSessions.Select(s => s.Subject?.Name ?? "")
                        .Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

                    return new RecommendedTutorDto
                    {
                        TutorId = tutorId,
                        TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "Unknown",
                        AverageRating = tutorReviews?.Any() == true ? Math.Round(tutorReviews.Average(r => r.Rating), 2) : 0,
                        TotalSessions = tutorSessions.Count,
                        HourlyRate = profile?.HourlyRate ?? 0,
                        SubjectNames = subjectNames
                    };
                })
                .OrderByDescending(t => t.AverageRating)
                .ThenByDescending(t => t.TotalSessions)
                .Take(maxResults)
                .ToList();

            return Result<List<RecommendedTutorDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended tutors");
            return Result<List<RecommendedTutorDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}

// Feature 13: Student learning progress
public class GetStudentProgressQueryHandler : IRequestHandler<GetStudentProgressQuery, Result<StudentProgressDto>>
{
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetStudentProgressQueryHandler> _logger;

    public GetStudentProgressQueryHandler(
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        ICurrentUserService currentUserService,
        ILogger<GetStudentProgressQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<StudentProgressDto>> Handle(GetStudentProgressQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentProgressDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var completedBookings = (await _bookingRepository.FindAsync(
                b => b.StudentId == userId.Value && b.BookingStatus == BookingStatus.Completed,
                cancellationToken)).ToList();

            if (!completedBookings.Any())
            {
                return Result<StudentProgressDto>.SuccessResult(new StudentProgressDto
                {
                    TotalSessions = 0,
                    TotalHours = 0,
                    TutorsCount = 0,
                    ThisMonth = 0,
                    LastMonth = 0,
                    SubjectBreakdown = new List<SubjectBreakdownDto>()
                });
            }

            var sessionIds = completedBookings.Select(b => b.SessionId).Distinct().ToList();

            var sessions = await _sessionRepository.GetQueryable()
                .Include(s => s.Subject)
                .Where(s => sessionIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var thisMonthBookings = completedBookings.Count(b =>
            {
                var session = sessions.FirstOrDefault(s => s.Id == b.SessionId);
                return session != null && session.ScheduledAt >= thisMonthStart;
            });

            var lastMonthBookings = completedBookings.Count(b =>
            {
                var session = sessions.FirstOrDefault(s => s.Id == b.SessionId);
                return session != null && session.ScheduledAt >= lastMonthStart && session.ScheduledAt < thisMonthStart;
            });

            var totalMinutes = sessions.Sum(s => s.Duration);
            var distinctTutors = sessions.Select(s => s.TutorId).Distinct().Count();

            var subjectBreakdown = sessions
                .GroupBy(s => new { s.SubjectId, SubjectName = s.Subject?.Name ?? "Unknown" })
                .Select(g => new SubjectBreakdownDto
                {
                    SubjectName = g.Key.SubjectName,
                    SessionCount = g.Count(),
                    HoursStudied = Math.Round(g.Sum(s => s.Duration) / 60.0, 2)
                })
                .OrderByDescending(s => s.SessionCount)
                .ToList();

            var dto = new StudentProgressDto
            {
                TotalSessions = completedBookings.Count,
                TotalHours = Math.Round(totalMinutes / 60.0, 2),
                TutorsCount = distinctTutors,
                ThisMonth = thisMonthBookings,
                LastMonth = lastMonthBookings,
                SubjectBreakdown = subjectBreakdown
            };

            return Result<StudentProgressDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student progress");
            return Result<StudentProgressDto>.FailureResult("ERROR", ex.Message);
        }
    }
}
