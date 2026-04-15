using LiveExpert.Application.Common;
using LiveExpert.Application.Features.StudentRatings.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.StudentRatings.Handlers;

// Feature 4: Mutual rating - tutor rates student

public class RateStudentCommandHandler : IRequestHandler<RateStudentCommand, Result<StudentRatingDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<StudentRating> _ratingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RateStudentCommandHandler> _logger;

    public RateStudentCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<StudentRating> ratingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<RateStudentCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _ratingRepository = ratingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<StudentRatingDto>> Handle(RateStudentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<StudentRatingDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
                return Result<StudentRatingDto>.FailureResult("NOT_FOUND", "Session not found");

            if (session.TutorId != userId.Value)
                return Result<StudentRatingDto>.FailureResult("FORBIDDEN", "Only the session tutor can rate students");

            // Validate student attended the session
            var booking = await _bookingRepository.FirstOrDefaultAsync(
                b => b.SessionId == request.SessionId && b.StudentId == request.StudentId, cancellationToken);
            if (booking == null)
                return Result<StudentRatingDto>.FailureResult("NOT_FOUND", "Student booking not found for this session");

            // One rating per session/student
            var existing = await _ratingRepository.FirstOrDefaultAsync(
                r => r.SessionId == request.SessionId && r.StudentId == request.StudentId, cancellationToken);
            if (existing != null)
                return Result<StudentRatingDto>.FailureResult("CONFLICT", "You have already rated this student for this session");

            if (request.Rating < 1 || request.Rating > 5)
                return Result<StudentRatingDto>.FailureResult("VALIDATION_ERROR", "Rating must be between 1 and 5");

            if (request.Punctuality < 1 || request.Punctuality > 5)
                return Result<StudentRatingDto>.FailureResult("VALIDATION_ERROR", "Punctuality must be between 1 and 5");

            if (request.Preparedness < 1 || request.Preparedness > 5)
                return Result<StudentRatingDto>.FailureResult("VALIDATION_ERROR", "Preparedness must be between 1 and 5");

            var rating = new StudentRating
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                TutorId = userId.Value,
                StudentId = request.StudentId,
                Rating = request.Rating,
                Comment = request.Comment,
                Punctuality = request.Punctuality,
                Preparedness = request.Preparedness,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _ratingRepository.AddAsync(rating, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<StudentRatingDto>.SuccessResult(new StudentRatingDto
            {
                Id = rating.Id,
                SessionId = rating.SessionId,
                TutorId = rating.TutorId,
                StudentId = rating.StudentId,
                Rating = rating.Rating,
                Comment = rating.Comment,
                Punctuality = rating.Punctuality,
                Preparedness = rating.Preparedness,
                CreatedAt = rating.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error rating student");
            return Result<StudentRatingDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetStudentRatingsQueryHandler : IRequestHandler<GetStudentRatingsQuery, Result<StudentRatingsResultDto>>
{
    private readonly IRepository<StudentRating> _ratingRepository;
    private readonly ILogger<GetStudentRatingsQueryHandler> _logger;

    public GetStudentRatingsQueryHandler(
        IRepository<StudentRating> ratingRepository,
        ILogger<GetStudentRatingsQueryHandler> logger)
    {
        _ratingRepository = ratingRepository;
        _logger = logger;
    }

    public async Task<Result<StudentRatingsResultDto>> Handle(GetStudentRatingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var ratings = (await _ratingRepository.FindAsync(
                r => r.StudentId == request.StudentId, cancellationToken)).ToList();

            var result = new StudentRatingsResultDto
            {
                StudentId = request.StudentId,
                TotalRatings = ratings.Count,
                AverageRating = ratings.Any() ? Math.Round(ratings.Average(r => r.Rating), 2) : 0,
                AveragePunctuality = ratings.Any() ? Math.Round(ratings.Average(r => r.Punctuality), 2) : 0,
                AveragePreparedness = ratings.Any() ? Math.Round(ratings.Average(r => r.Preparedness), 2) : 0,
                Ratings = ratings.OrderByDescending(r => r.CreatedAt).Select(r => new StudentRatingDto
                {
                    Id = r.Id,
                    SessionId = r.SessionId,
                    TutorId = r.TutorId,
                    StudentId = r.StudentId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Punctuality = r.Punctuality,
                    Preparedness = r.Preparedness,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return Result<StudentRatingsResultDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student ratings");
            return Result<StudentRatingsResultDto>.FailureResult("ERROR", ex.Message);
        }
    }
}
