using LiveExpert.Application.Common;
using MediatR;

namespace LiveExpert.Application.Features.StudentRatings.Commands;

// Feature 4: Mutual rating - tutor rates student

public class RateStudentCommand : IRequest<Result<StudentRatingDto>>
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public int Punctuality { get; set; }
    public int Preparedness { get; set; }
}

public class GetStudentRatingsQuery : IRequest<Result<StudentRatingsResultDto>>
{
    public Guid StudentId { get; set; }
}

public class StudentRatingDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid TutorId { get; set; }
    public Guid StudentId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public int Punctuality { get; set; }
    public int Preparedness { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentRatingsResultDto
{
    public Guid StudentId { get; set; }
    public double AverageRating { get; set; }
    public double AveragePunctuality { get; set; }
    public double AveragePreparedness { get; set; }
    public int TotalRatings { get; set; }
    public List<StudentRatingDto> Ratings { get; set; } = new();
}
