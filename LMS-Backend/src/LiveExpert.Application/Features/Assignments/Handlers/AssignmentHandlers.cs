using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Assignments.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.Assignments.Handlers;

// Feature 3: Homework / Assignments

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Result<AssignmentDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionAssignment> _assignmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAssignmentCommandHandler> _logger;

    public CreateAssignmentCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionAssignment> assignmentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CreateAssignmentCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _assignmentRepository = assignmentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AssignmentDto>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<AssignmentDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
                return Result<AssignmentDto>.FailureResult("NOT_FOUND", "Session not found");

            if (session.TutorId != userId.Value)
                return Result<AssignmentDto>.FailureResult("FORBIDDEN", "Only the session tutor can create assignments");

            var assignment = new SessionAssignment
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                TutorId = userId.Value,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                FileUrl = request.FileUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _assignmentRepository.AddAsync(assignment, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<AssignmentDto>.SuccessResult(MapToDto(assignment));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating assignment");
            return Result<AssignmentDto>.FailureResult("ERROR", ex.Message);
        }
    }

    private static AssignmentDto MapToDto(SessionAssignment a) => new()
    {
        Id = a.Id,
        SessionId = a.SessionId,
        Title = a.Title,
        Description = a.Description,
        DueDate = a.DueDate,
        FileUrl = a.FileUrl,
        CreatedAt = a.CreatedAt
    };
}

public class SubmitAssignmentCommandHandler : IRequestHandler<SubmitAssignmentCommand, Result<AssignmentSubmissionDto>>
{
    private readonly IRepository<SessionAssignment> _assignmentRepository;
    private readonly IRepository<AssignmentSubmission> _submissionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubmitAssignmentCommandHandler> _logger;

    public SubmitAssignmentCommandHandler(
        IRepository<SessionAssignment> assignmentRepository,
        IRepository<AssignmentSubmission> submissionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<SubmitAssignmentCommandHandler> logger)
    {
        _assignmentRepository = assignmentRepository;
        _submissionRepository = submissionRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AssignmentSubmissionDto>> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<AssignmentSubmissionDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken);
            if (assignment == null)
                return Result<AssignmentSubmissionDto>.FailureResult("NOT_FOUND", "Assignment not found");

            var existing = await _submissionRepository.FirstOrDefaultAsync(
                s => s.AssignmentId == request.AssignmentId && s.StudentId == userId.Value, cancellationToken);
            if (existing != null)
                return Result<AssignmentSubmissionDto>.FailureResult("CONFLICT", "You have already submitted this assignment");

            var submission = new AssignmentSubmission
            {
                Id = Guid.NewGuid(),
                AssignmentId = request.AssignmentId,
                StudentId = userId.Value,
                Content = request.Content,
                FileUrl = request.FileUrl,
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _submissionRepository.AddAsync(submission, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<AssignmentSubmissionDto>.SuccessResult(MapToDto(submission));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error submitting assignment");
            return Result<AssignmentSubmissionDto>.FailureResult("ERROR", ex.Message);
        }
    }

    private static AssignmentSubmissionDto MapToDto(AssignmentSubmission s) => new()
    {
        Id = s.Id,
        AssignmentId = s.AssignmentId,
        StudentId = s.StudentId,
        Content = s.Content,
        FileUrl = s.FileUrl,
        SubmittedAt = s.SubmittedAt,
        FeedbackText = s.FeedbackText,
        FeedbackAt = s.FeedbackAt,
        Grade = s.Grade,
        Status = s.Status
    };
}

public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand, Result<AssignmentSubmissionDto>>
{
    private readonly IRepository<AssignmentSubmission> _submissionRepository;
    private readonly IRepository<SessionAssignment> _assignmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GradeSubmissionCommandHandler> _logger;

    public GradeSubmissionCommandHandler(
        IRepository<AssignmentSubmission> submissionRepository,
        IRepository<SessionAssignment> assignmentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<GradeSubmissionCommandHandler> logger)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AssignmentSubmissionDto>> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<AssignmentSubmissionDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId, cancellationToken);
            if (submission == null)
                return Result<AssignmentSubmissionDto>.FailureResult("NOT_FOUND", "Submission not found");

            var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);
            if (assignment == null || assignment.TutorId != userId.Value)
                return Result<AssignmentSubmissionDto>.FailureResult("FORBIDDEN", "Only the assignment tutor can grade submissions");

            submission.FeedbackText = request.FeedbackText;
            submission.FeedbackAt = DateTime.UtcNow;
            submission.Grade = request.Grade;
            submission.Status = SubmissionStatus.Graded;
            submission.UpdatedAt = DateTime.UtcNow;

            await _submissionRepository.UpdateAsync(submission, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<AssignmentSubmissionDto>.SuccessResult(new AssignmentSubmissionDto
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentId = submission.StudentId,
                Content = submission.Content,
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt,
                FeedbackText = submission.FeedbackText,
                FeedbackAt = submission.FeedbackAt,
                Grade = submission.Grade,
                Status = submission.Status
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error grading submission");
            return Result<AssignmentSubmissionDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetSessionAssignmentsQueryHandler : IRequestHandler<GetSessionAssignmentsQuery, Result<List<AssignmentDto>>>
{
    private readonly IRepository<SessionAssignment> _assignmentRepository;
    private readonly ILogger<GetSessionAssignmentsQueryHandler> _logger;

    public GetSessionAssignmentsQueryHandler(
        IRepository<SessionAssignment> assignmentRepository,
        ILogger<GetSessionAssignmentsQueryHandler> logger)
    {
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task<Result<List<AssignmentDto>>> Handle(GetSessionAssignmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var assignments = await _assignmentRepository.FindAsync(
                a => a.SessionId == request.SessionId, cancellationToken);

            var dtos = assignments.OrderByDescending(a => a.CreatedAt).Select(a => new AssignmentDto
            {
                Id = a.Id,
                SessionId = a.SessionId,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                FileUrl = a.FileUrl,
                CreatedAt = a.CreatedAt
            }).ToList();

            return Result<List<AssignmentDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session assignments");
            return Result<List<AssignmentDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetAssignmentSubmissionsQueryHandler : IRequestHandler<GetAssignmentSubmissionsQuery, Result<List<AssignmentSubmissionDto>>>
{
    private readonly IRepository<AssignmentSubmission> _submissionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAssignmentSubmissionsQueryHandler> _logger;

    public GetAssignmentSubmissionsQueryHandler(
        IRepository<AssignmentSubmission> submissionRepository,
        ICurrentUserService currentUserService,
        ILogger<GetAssignmentSubmissionsQueryHandler> logger)
    {
        _submissionRepository = submissionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<AssignmentSubmissionDto>>> Handle(GetAssignmentSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<AssignmentSubmissionDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var submissions = await _submissionRepository.FindAsync(
                s => s.AssignmentId == request.AssignmentId, cancellationToken);

            var dtos = submissions.OrderByDescending(s => s.SubmittedAt).Select(s => new AssignmentSubmissionDto
            {
                Id = s.Id,
                AssignmentId = s.AssignmentId,
                StudentId = s.StudentId,
                Content = s.Content,
                FileUrl = s.FileUrl,
                SubmittedAt = s.SubmittedAt,
                FeedbackText = s.FeedbackText,
                FeedbackAt = s.FeedbackAt,
                Grade = s.Grade,
                Status = s.Status
            }).ToList();

            return Result<List<AssignmentSubmissionDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment submissions");
            return Result<List<AssignmentSubmissionDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}
