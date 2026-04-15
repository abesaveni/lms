using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.LearningPaths.Commands;

// Feature 14: Learning paths

public class LearningPathStepInput
{
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
}

public class CreateLearningPathCommand : IRequest<Result<LearningPathDto>>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? SubjectId { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<LearningPathStepInput> Steps { get; set; } = new();
}

public class PublishLearningPathCommand : IRequest<Result<LearningPathDto>>
{
    public Guid PathId { get; set; }
}

public class EnrollInLearningPathCommand : IRequest<Result<LearningPathEnrollmentDto>>
{
    public Guid PathId { get; set; }
}

public class CompleteStepCommand : IRequest<Result<LearningPathEnrollmentDto>>
{
    public Guid EnrollmentId { get; set; }
    public int StepNumber { get; set; }
}

public class GetLearningPathsQuery : IRequest<Result<List<LearningPathDto>>>
{
    public Guid? TutorId { get; set; }
}

public class GetLearningPathQuery : IRequest<Result<LearningPathDetailDto>>
{
    public Guid PathId { get; set; }
}

public class GetMyLearningEnrollmentsQuery : IRequest<Result<List<LearningPathEnrollmentDto>>>
{
}

public class LearningPathDto
{
    public Guid Id { get; set; }
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? SubjectId { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsPublished { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int StepCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LearningPathStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
}

public class LearningPathDetailDto : LearningPathDto
{
    public List<LearningPathStepDto> Steps { get; set; } = new();
}

public class LearningPathEnrollmentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid LearningPathId { get; set; }
    public string PathTitle { get; set; } = string.Empty;
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public int CurrentStep { get; set; }
    public DateTime EnrolledAt { get; set; }
    public LearningEnrollmentStatus Status { get; set; }
}
