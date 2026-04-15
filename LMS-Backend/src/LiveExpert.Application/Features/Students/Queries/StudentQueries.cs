using LiveExpert.Application.Common;
using MediatR;

namespace LiveExpert.Application.Features.Students.Queries;

// Feature 5: Re-book with same tutor
public class GetRecentTutorsQuery : IRequest<Result<List<RecentTutorDto>>>
{
}

public class RecentTutorDto
{
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public DateTime LastSessionDate { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
}

// Feature 11: Tutor recommendations
public class GetRecommendedTutorsQuery : IRequest<Result<List<RecommendedTutorDto>>>
{
    public Guid? SubjectId { get; set; }
    public int MaxResults { get; set; } = 10;
}

public class RecommendedTutorDto
{
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalSessions { get; set; }
    public decimal HourlyRate { get; set; }
    public List<string> SubjectNames { get; set; } = new();
}

// Feature 13: Student learning progress
public class GetStudentProgressQuery : IRequest<Result<StudentProgressDto>>
{
}

public class StudentProgressDto
{
    public int TotalSessions { get; set; }
    public double TotalHours { get; set; }
    public int TutorsCount { get; set; }
    public int ThisMonth { get; set; }
    public int LastMonth { get; set; }
    public List<SubjectBreakdownDto> SubjectBreakdown { get; set; } = new();
}

public class SubjectBreakdownDto
{
    public string SubjectName { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public double HoursStudied { get; set; }
}
