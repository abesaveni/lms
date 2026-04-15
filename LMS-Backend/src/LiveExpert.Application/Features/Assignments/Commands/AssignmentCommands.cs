using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Assignments.Commands;

// Feature 3: Homework / Assignments

public class CreateAssignmentCommand : IRequest<Result<AssignmentDto>>
{
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? FileUrl { get; set; }
}

public class SubmitAssignmentCommand : IRequest<Result<AssignmentSubmissionDto>>
{
    public Guid AssignmentId { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
}

public class GradeSubmissionCommand : IRequest<Result<AssignmentSubmissionDto>>
{
    public Guid SubmissionId { get; set; }
    public string? FeedbackText { get; set; }
    public int? Grade { get; set; }
}

public class GetSessionAssignmentsQuery : IRequest<Result<List<AssignmentDto>>>
{
    public Guid SessionId { get; set; }
}

public class GetAssignmentSubmissionsQuery : IRequest<Result<List<AssignmentSubmissionDto>>>
{
    public Guid AssignmentId { get; set; }
}

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignmentSubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? FeedbackText { get; set; }
    public DateTime? FeedbackAt { get; set; }
    public int? Grade { get; set; }
    public SubmissionStatus Status { get; set; }
}
