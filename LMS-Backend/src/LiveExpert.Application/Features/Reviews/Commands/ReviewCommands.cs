using LiveExpert.Application.Common;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Reviews.Commands;

// Submit Review Command
public class SubmitReviewCommand : IRequest<Result<Guid>>
{
    public Guid TutorId { get; set; }
    public Guid SessionId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

// Get Reviews Query
public class GetReviewsQuery : IRequest<Result<PaginatedResult<ReviewDto>>>
{
    public Guid? TutorId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentImage { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? TutorResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

// Respond to Review Command
public class RespondToReviewCommand : IRequest<Result>
{
    public Guid ReviewId { get; set; }
    public string Response { get; set; } = string.Empty;
}
