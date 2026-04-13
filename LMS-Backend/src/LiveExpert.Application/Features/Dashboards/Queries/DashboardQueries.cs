using LiveExpert.Application.Common;
using MediatR;

namespace LiveExpert.Application.Features.Dashboards.Queries;

// Student Dashboard Query
public class GetStudentDashboardQuery : IRequest<Result<StudentDashboardDto>>
{
}

public class StudentDashboardDto
{
    public int TotalBookings { get; set; }
    public int CompletedSessions { get; set; }
    public int UpcomingSessionsCount { get; set; }
    public int TotalBonusPoints { get; set; }
    public List<UpcomingSessionDto> UpcomingSessions { get; set; } = new();
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class UpcomingSessionDto
{
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TutorName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string MeetingLink { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

// Student Statistics Query
public class GetStudentStatsQuery : IRequest<Result<StudentStatsDto>>
{
}

public class StudentStatsDto
{
    public int TotalSessionsAttended { get; set; }
    public double TotalHoursLearned { get; set; }
    public decimal TotalAmountSpent { get; set; }
    public Dictionary<string, int> SessionsBySubject { get; set; } = new();
    public List<MonthlyActivityDto> MonthlyActivity { get; set; } = new();
}

public class MonthlyActivityDto
{
    public string Month { get; set; } = string.Empty;
    public int Sessions { get; set; }
    public decimal AmountSpent { get; set; }
}

// Tutor Dashboard Query
public class GetTutorDashboardQuery : IRequest<Result<TutorDashboardDto>>
{
}

public class TutorDashboardDto
{
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int UpcomingSessionsCount { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TotalStudents { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<UpcomingSessionDto> UpcomingSessions { get; set; } = new();
    public List<RecentBookingDto> RecentBookings { get; set; } = new();
}

public class RecentBookingDto
{
    public Guid BookingId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
    public DateTime ScheduledAt { get; set; }
}

// Tutor Statistics Query
public class GetTutorStatsQuery : IRequest<Result<TutorStatsDto>>
{
}

public class TutorStatsDto
{
    public int TotalSessionsConducted { get; set; }
    public int TotalHoursTaught { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public int UniqueStudents { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<string, int> SessionsBySubject { get; set; } = new();
    public List<MonthlyEarningsDto> MonthlyEarnings { get; set; } = new();
}

public class MonthlyEarningsDto
{
    public string Month { get; set; } = string.Empty;
    public int Sessions { get; set; }
    public decimal Earnings { get; set; }
}
