using LiveExpert.Application.Common;
using LiveExpert.Application.Features.LearningPaths.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.LearningPaths.Handlers;

// Feature 14: Learning paths

public class CreateLearningPathCommandHandler : IRequestHandler<CreateLearningPathCommand, Result<LearningPathDto>>
{
    private readonly IRepository<LearningPath> _pathRepository;
    private readonly IRepository<LearningPathStep> _stepRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateLearningPathCommandHandler> _logger;

    public CreateLearningPathCommandHandler(
        IRepository<LearningPath> pathRepository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CreateLearningPathCommandHandler> logger)
    {
        _pathRepository = pathRepository;
        _stepRepository = stepRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LearningPathDto>> Handle(CreateLearningPathCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<LearningPathDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var path = new LearningPath
            {
                Id = Guid.NewGuid(),
                TutorId = userId.Value,
                Title = request.Title,
                Description = request.Description,
                SubjectId = request.SubjectId,
                TotalPrice = request.TotalPrice,
                IsPublished = false,
                ThumbnailUrl = request.ThumbnailUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _pathRepository.AddAsync(path, cancellationToken);

            var steps = request.Steps.Select(s => new LearningPathStep
            {
                Id = Guid.NewGuid(),
                LearningPathId = path.Id,
                StepNumber = s.StepNumber,
                Title = s.Title,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            await _stepRepository.AddRangeAsync(steps, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var tutor = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

            return Result<LearningPathDto>.SuccessResult(new LearningPathDto
            {
                Id = path.Id,
                TutorId = path.TutorId,
                TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "",
                Title = path.Title,
                Description = path.Description,
                SubjectId = path.SubjectId,
                TotalPrice = path.TotalPrice,
                IsPublished = path.IsPublished,
                ThumbnailUrl = path.ThumbnailUrl,
                StepCount = steps.Count,
                CreatedAt = path.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating learning path");
            return Result<LearningPathDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class PublishLearningPathCommandHandler : IRequestHandler<PublishLearningPathCommand, Result<LearningPathDto>>
{
    private readonly IRepository<LearningPath> _pathRepository;
    private readonly IRepository<LearningPathStep> _stepRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PublishLearningPathCommandHandler> _logger;

    public PublishLearningPathCommandHandler(
        IRepository<LearningPath> pathRepository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<PublishLearningPathCommandHandler> logger)
    {
        _pathRepository = pathRepository;
        _stepRepository = stepRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LearningPathDto>> Handle(PublishLearningPathCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<LearningPathDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var path = await _pathRepository.GetByIdAsync(request.PathId, cancellationToken);
            if (path == null)
                return Result<LearningPathDto>.FailureResult("NOT_FOUND", "Learning path not found");

            if (path.TutorId != userId.Value)
                return Result<LearningPathDto>.FailureResult("FORBIDDEN", "Only the path creator can publish it");

            path.IsPublished = true;
            path.UpdatedAt = DateTime.UtcNow;

            await _pathRepository.UpdateAsync(path, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var tutor = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            var stepCount = await _stepRepository.CountAsync(s => s.LearningPathId == path.Id, cancellationToken);

            return Result<LearningPathDto>.SuccessResult(new LearningPathDto
            {
                Id = path.Id,
                TutorId = path.TutorId,
                TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "",
                Title = path.Title,
                Description = path.Description,
                SubjectId = path.SubjectId,
                TotalPrice = path.TotalPrice,
                IsPublished = path.IsPublished,
                ThumbnailUrl = path.ThumbnailUrl,
                StepCount = stepCount,
                CreatedAt = path.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error publishing learning path");
            return Result<LearningPathDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class EnrollInLearningPathCommandHandler : IRequestHandler<EnrollInLearningPathCommand, Result<LearningPathEnrollmentDto>>
{
    private readonly IRepository<LearningPath> _pathRepository;
    private readonly IRepository<LearningPathStep> _stepRepository;
    private readonly IRepository<LearningPathEnrollment> _enrollmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnrollInLearningPathCommandHandler> _logger;

    public EnrollInLearningPathCommandHandler(
        IRepository<LearningPath> pathRepository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<LearningPathEnrollment> enrollmentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<EnrollInLearningPathCommandHandler> logger)
    {
        _pathRepository = pathRepository;
        _stepRepository = stepRepository;
        _enrollmentRepository = enrollmentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LearningPathEnrollmentDto>> Handle(EnrollInLearningPathCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<LearningPathEnrollmentDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var path = await _pathRepository.GetByIdAsync(request.PathId, cancellationToken);
            if (path == null || !path.IsPublished)
                return Result<LearningPathEnrollmentDto>.FailureResult("NOT_FOUND", "Learning path not found or not published");

            var existing = await _enrollmentRepository.FirstOrDefaultAsync(
                e => e.LearningPathId == request.PathId && e.StudentId == userId.Value, cancellationToken);
            if (existing != null)
                return Result<LearningPathEnrollmentDto>.FailureResult("CONFLICT", "You are already enrolled in this learning path");

            var stepCount = await _stepRepository.CountAsync(s => s.LearningPathId == request.PathId, cancellationToken);

            var enrollment = new LearningPathEnrollment
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                LearningPathId = request.PathId,
                CompletedSteps = 0,
                CurrentStep = 1,
                EnrolledAt = DateTime.UtcNow,
                Status = LearningEnrollmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<LearningPathEnrollmentDto>.SuccessResult(new LearningPathEnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                LearningPathId = enrollment.LearningPathId,
                PathTitle = path.Title,
                CompletedSteps = enrollment.CompletedSteps,
                TotalSteps = stepCount,
                CurrentStep = enrollment.CurrentStep,
                EnrolledAt = enrollment.EnrolledAt,
                Status = enrollment.Status
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error enrolling in learning path");
            return Result<LearningPathEnrollmentDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class CompleteStepCommandHandler : IRequestHandler<CompleteStepCommand, Result<LearningPathEnrollmentDto>>
{
    private readonly IRepository<LearningPathEnrollment> _enrollmentRepository;
    private readonly IRepository<LearningPath> _pathRepository;
    private readonly IRepository<LearningPathStep> _stepRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteStepCommandHandler> _logger;

    public CompleteStepCommandHandler(
        IRepository<LearningPathEnrollment> enrollmentRepository,
        IRepository<LearningPath> pathRepository,
        IRepository<LearningPathStep> stepRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CompleteStepCommandHandler> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _pathRepository = pathRepository;
        _stepRepository = stepRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LearningPathEnrollmentDto>> Handle(CompleteStepCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<LearningPathEnrollmentDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
                return Result<LearningPathEnrollmentDto>.FailureResult("NOT_FOUND", "Enrollment not found");

            if (enrollment.StudentId != userId.Value)
                return Result<LearningPathEnrollmentDto>.FailureResult("FORBIDDEN", "Not your enrollment");

            if (enrollment.Status != LearningEnrollmentStatus.Active)
                return Result<LearningPathEnrollmentDto>.FailureResult("CONFLICT", "Enrollment is not active");

            var path = await _pathRepository.GetByIdAsync(enrollment.LearningPathId, cancellationToken);
            var totalSteps = await _stepRepository.CountAsync(s => s.LearningPathId == enrollment.LearningPathId, cancellationToken);

            if (request.StepNumber != enrollment.CurrentStep)
                return Result<LearningPathEnrollmentDto>.FailureResult("VALIDATION_ERROR",
                    $"Expected current step {enrollment.CurrentStep}, got {request.StepNumber}");

            enrollment.CompletedSteps++;
            enrollment.CurrentStep = request.StepNumber + 1;

            if (enrollment.CompletedSteps >= totalSteps)
                enrollment.Status = LearningEnrollmentStatus.Completed;

            enrollment.UpdatedAt = DateTime.UtcNow;

            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<LearningPathEnrollmentDto>.SuccessResult(new LearningPathEnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                LearningPathId = enrollment.LearningPathId,
                PathTitle = path?.Title ?? "",
                CompletedSteps = enrollment.CompletedSteps,
                TotalSteps = totalSteps,
                CurrentStep = enrollment.CurrentStep,
                EnrolledAt = enrollment.EnrolledAt,
                Status = enrollment.Status
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error completing step");
            return Result<LearningPathEnrollmentDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetLearningPathsQueryHandler : IRequestHandler<GetLearningPathsQuery, Result<List<LearningPathDto>>>
{
    private readonly IRepository<LearningPath> _pathRepository;
    private readonly IRepository<LearningPathStep> _stepRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<GetLearningPathsQueryHandler> _logger;

    public GetLearningPathsQueryHandler(
        IRepository<LearningPath> pathRepository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<User> userRepository,
        ILogger<GetLearningPathsQueryHandler> logger)
    {
        _pathRepository = pathRepository;
        _stepRepository = stepRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<List<LearningPathDto>>> Handle(GetLearningPathsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<LearningPath> paths;
            if (request.TutorId.HasValue)
                paths = await _pathRepository.FindAsync(p => p.TutorId == request.TutorId.Value, cancellationToken);
            else
                paths = await _pathRepository.FindAsync(p => p.IsPublished, cancellationToken);

            var pathList = paths.OrderByDescending(p => p.CreatedAt).ToList();
            var tutorIds = pathList.Select(p => p.TutorId).Distinct().ToList();
            var tutors = (await _userRepository.FindAsync(u => tutorIds.Contains(u.Id), cancellationToken))
                .ToDictionary(u => u.Id);

            var dtos = new List<LearningPathDto>();
            foreach (var path in pathList)
            {
                var stepCount = await _stepRepository.CountAsync(s => s.LearningPathId == path.Id, cancellationToken);
                tutors.TryGetValue(path.TutorId, out var tutor);
                dtos.Add(new LearningPathDto
                {
                    Id = path.Id,
                    TutorId = path.TutorId,
                    TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "",
                    Title = path.Title,
                    Description = path.Description,
                    SubjectId = path.SubjectId,
                    TotalPrice = path.TotalPrice,
                    IsPublished = path.IsPublished,
                    ThumbnailUrl = path.ThumbnailUrl,
                    StepCount = stepCount,
                    CreatedAt = path.CreatedAt
                });
            }

            return Result<List<LearningPathDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learning paths");
            return Result<List<LearningPathDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetLearningPathQueryHandler : IRequestHandler<GetLearningPathQuery, Result<LearningPathDetailDto>>
{
    private readonly IRepository<LearningPath> _pathRepository;
    private readonly IRepository<LearningPathStep> _stepRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<GetLearningPathQueryHandler> _logger;

    public GetLearningPathQueryHandler(
        IRepository<LearningPath> pathRepository,
        IRepository<LearningPathStep> stepRepository,
        IRepository<User> userRepository,
        ILogger<GetLearningPathQueryHandler> logger)
    {
        _pathRepository = pathRepository;
        _stepRepository = stepRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<LearningPathDetailDto>> Handle(GetLearningPathQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var path = await _pathRepository.GetByIdAsync(request.PathId, cancellationToken);
            if (path == null)
                return Result<LearningPathDetailDto>.FailureResult("NOT_FOUND", "Learning path not found");

            var steps = (await _stepRepository.FindAsync(s => s.LearningPathId == path.Id, cancellationToken))
                .OrderBy(s => s.StepNumber).ToList();

            var tutor = await _userRepository.GetByIdAsync(path.TutorId, cancellationToken);

            return Result<LearningPathDetailDto>.SuccessResult(new LearningPathDetailDto
            {
                Id = path.Id,
                TutorId = path.TutorId,
                TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "",
                Title = path.Title,
                Description = path.Description,
                SubjectId = path.SubjectId,
                TotalPrice = path.TotalPrice,
                IsPublished = path.IsPublished,
                ThumbnailUrl = path.ThumbnailUrl,
                StepCount = steps.Count,
                CreatedAt = path.CreatedAt,
                Steps = steps.Select(s => new LearningPathStepDto
                {
                    Id = s.Id,
                    StepNumber = s.StepNumber,
                    Title = s.Title,
                    Description = s.Description,
                    DurationMinutes = s.DurationMinutes
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learning path");
            return Result<LearningPathDetailDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetMyLearningEnrollmentsQueryHandler : IRequestHandler<GetMyLearningEnrollmentsQuery, Result<List<LearningPathEnrollmentDto>>>
{
    private readonly IRepository<LearningPathEnrollment> _enrollmentRepository;
    private readonly IRepository<LearningPath> _pathRepository;
    private readonly IRepository<LearningPathStep> _stepRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyLearningEnrollmentsQueryHandler> _logger;

    public GetMyLearningEnrollmentsQueryHandler(
        IRepository<LearningPathEnrollment> enrollmentRepository,
        IRepository<LearningPath> pathRepository,
        IRepository<LearningPathStep> stepRepository,
        ICurrentUserService currentUserService,
        ILogger<GetMyLearningEnrollmentsQueryHandler> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _pathRepository = pathRepository;
        _stepRepository = stepRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<LearningPathEnrollmentDto>>> Handle(GetMyLearningEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<LearningPathEnrollmentDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var enrollments = (await _enrollmentRepository.FindAsync(e => e.StudentId == userId.Value, cancellationToken))
                .OrderByDescending(e => e.EnrolledAt).ToList();

            var pathIds = enrollments.Select(e => e.LearningPathId).Distinct().ToList();
            var paths = (await _pathRepository.FindAsync(p => pathIds.Contains(p.Id), cancellationToken))
                .ToDictionary(p => p.Id);

            var dtos = new List<LearningPathEnrollmentDto>();
            foreach (var e in enrollments)
            {
                paths.TryGetValue(e.LearningPathId, out var path);
                var stepCount = await _stepRepository.CountAsync(s => s.LearningPathId == e.LearningPathId, cancellationToken);
                dtos.Add(new LearningPathEnrollmentDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    LearningPathId = e.LearningPathId,
                    PathTitle = path?.Title ?? "",
                    CompletedSteps = e.CompletedSteps,
                    TotalSteps = stepCount,
                    CurrentStep = e.CurrentStep,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status
                });
            }

            return Result<List<LearningPathEnrollmentDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting learning enrollments");
            return Result<List<LearningPathEnrollmentDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}
