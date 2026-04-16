using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Reviews.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Reviews.Handlers;

// Submit Review Handler
public class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, Result<Guid>>
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public SubmitReviewCommandHandler(
        IRepository<Review> reviewRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _tutorRepository = tutorRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<Guid>.FailureResult("UNAUTHORIZED", "User not authenticated");

        // Validate rating
        if (request.Rating < 1 || request.Rating > 5)
            return Result<Guid>.FailureResult("INVALID_RATING", "Rating must be between 1 and 5");

        // Verify session booking exists and student attended
        var booking = await _bookingRepository.FirstOrDefaultAsync(
            b => b.SessionId == request.SessionId && b.StudentId == userId.Value,
            cancellationToken);

        if (booking == null)
            return Result<Guid>.FailureResult("NOT_FOUND", "Session booking not found");

        if (!booking.AttendanceMarked)
            return Result<Guid>.FailureResult("INVALID_STATUS", "Can only review sessions you attended");

        // Resolve TutorId from the session when not provided by the client
        if (request.TutorId == Guid.Empty)
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
                return Result<Guid>.FailureResult("NOT_FOUND", "Session not found");
            request.TutorId = session.TutorId;
        }

        // Check if already reviewed
        var existingReview = await _reviewRepository.FirstOrDefaultAsync(
            r => r.SessionId == request.SessionId && r.StudentId == userId.Value,
            cancellationToken);

        if (existingReview != null)
            return Result<Guid>.FailureResult("ALREADY_REVIEWED", "You have already reviewed this session");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create review
            var review = new Review
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                TutorId = request.TutorId,
                SessionId = request.SessionId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review, cancellationToken);

            // Update tutor's average rating
            var tutorProfile = await _tutorRepository.FirstOrDefaultAsync(
                t => t.UserId == request.TutorId, cancellationToken);

            if (tutorProfile != null)
            {
                var allReviews = await _reviewRepository.FindAsync(
                    r => r.TutorId == request.TutorId, cancellationToken);
                
                var totalReviews = allReviews.Count() + 1;
                var totalRating = allReviews.Sum(r => r.Rating) + request.Rating;
                
                tutorProfile.AverageRating = (decimal)totalRating / totalReviews;
                tutorProfile.TotalReviews = totalReviews;
                tutorProfile.UpdatedAt = DateTime.UtcNow;

                await _tutorRepository.UpdateAsync(tutorProfile, cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Send notification to tutor
            await _notificationService.SendNotificationAsync(
                request.TutorId,
                "New Review Received",
                $"You received a {request.Rating}-star review from a student.",
                null,
                null
            );

            return Result<Guid>.SuccessResult(review.Id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

// Get Reviews Handler
public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, Result<PaginatedResult<ReviewDto>>>
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetReviewsQueryHandler(
        IRepository<Review> reviewRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<ReviewDto>>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = await _reviewRepository.GetAllAsync(cancellationToken);

        if (request.TutorId.HasValue)
            query = query.Where(r => r.TutorId == request.TutorId.Value);

        var allReviews = query.OrderByDescending(r => r.CreatedAt).ToList();

        var reviews = allReviews
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            var student = await _userRepository.GetByIdAsync(review.StudentId, cancellationToken);
            
            dtos.Add(new ReviewDto
            {
                Id = review.Id,
                StudentName = student?.Username ?? "Anonymous",
                StudentImage = student?.ProfileImageUrl,
                Rating = review.Rating,
                Comment = review.Comment,
                TutorResponse = review.TutorResponse,
                CreatedAt = review.CreatedAt,
                RespondedAt = review.RespondedAt
            });
        }

        var result = new PaginatedResult<ReviewDto>
        {
            Items = dtos,
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalRecords = allReviews.Count,
                TotalPages = (int)Math.Ceiling(allReviews.Count / (double)request.PageSize)
            }
        };

        return Result<PaginatedResult<ReviewDto>>.SuccessResult(result);
    }
}

// Respond to Review Handler
public class RespondToReviewCommandHandler : IRequestHandler<RespondToReviewCommand, Result>
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public RespondToReviewCommandHandler(
        IRepository<Review> reviewRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _reviewRepository = reviewRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(RespondToReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");

        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
            return Result.FailureResult("NOT_FOUND", "Review not found");

        if (review.TutorId != userId.Value)
            return Result.FailureResult("FORBIDDEN", "You can only respond to your own reviews");

        if (!string.IsNullOrEmpty(review.TutorResponse))
            return Result.FailureResult("ALREADY_RESPONDED", "You have already responded to this review");

        review.TutorResponse = request.Response;
        review.RespondedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify student
        await _notificationService.SendNotificationAsync(
            review.StudentId,
            "Tutor Responded to Your Review",
            "The tutor has responded to your review.",
            null,
            null
        );

        return Result.SuccessResult("Response added successfully");
    }
}
