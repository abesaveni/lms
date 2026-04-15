using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Inquiries.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.Inquiries.Handlers;

// Feature 12: Pre-booking inquiry

public class SendInquiryCommandHandler : IRequestHandler<SendInquiryCommand, Result<TutorInquiryDto>>
{
    private readonly IRepository<TutorInquiry> _inquiryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendInquiryCommandHandler> _logger;

    public SendInquiryCommandHandler(
        IRepository<TutorInquiry> inquiryRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<SendInquiryCommandHandler> logger)
    {
        _inquiryRepository = inquiryRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TutorInquiryDto>> Handle(SendInquiryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<TutorInquiryDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var tutor = await _userRepository.GetByIdAsync(request.TutorId, cancellationToken);
            if (tutor == null)
                return Result<TutorInquiryDto>.FailureResult("NOT_FOUND", "Tutor not found");

            var student = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

            var inquiry = new TutorInquiry
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                TutorId = request.TutorId,
                SubjectId = request.SubjectId,
                Message = request.Message,
                Status = InquiryStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _inquiryRepository.AddAsync(inquiry, cancellationToken);

            await _notificationService.SendNotificationAsync(
                request.TutorId,
                "New Inquiry",
                $"{student?.FirstName ?? "A student"} has sent you an inquiry.",
                NotificationType.NewMessage,
                $"/inquiries/{inquiry.Id}",
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<TutorInquiryDto>.SuccessResult(new TutorInquiryDto
            {
                Id = inquiry.Id,
                StudentId = inquiry.StudentId,
                StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "",
                TutorId = inquiry.TutorId,
                TutorName = $"{tutor.FirstName} {tutor.LastName}".Trim(),
                SubjectId = inquiry.SubjectId,
                Message = inquiry.Message,
                TutorReply = inquiry.TutorReply,
                RepliedAt = inquiry.RepliedAt,
                Status = inquiry.Status,
                CreatedAt = inquiry.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error sending inquiry");
            return Result<TutorInquiryDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class ReplyToInquiryCommandHandler : IRequestHandler<ReplyToInquiryCommand, Result<TutorInquiryDto>>
{
    private readonly IRepository<TutorInquiry> _inquiryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReplyToInquiryCommandHandler> _logger;

    public ReplyToInquiryCommandHandler(
        IRepository<TutorInquiry> inquiryRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<ReplyToInquiryCommandHandler> logger)
    {
        _inquiryRepository = inquiryRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TutorInquiryDto>> Handle(ReplyToInquiryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<TutorInquiryDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inquiry = await _inquiryRepository.GetByIdAsync(request.InquiryId, cancellationToken);
            if (inquiry == null)
                return Result<TutorInquiryDto>.FailureResult("NOT_FOUND", "Inquiry not found");

            if (inquiry.TutorId != userId.Value)
                return Result<TutorInquiryDto>.FailureResult("FORBIDDEN", "Only the tutor can reply to this inquiry");

            if (inquiry.Status == InquiryStatus.Closed)
                return Result<TutorInquiryDto>.FailureResult("CONFLICT", "This inquiry is already closed");

            inquiry.TutorReply = request.Reply;
            inquiry.RepliedAt = DateTime.UtcNow;
            inquiry.Status = InquiryStatus.Replied;
            inquiry.UpdatedAt = DateTime.UtcNow;

            await _inquiryRepository.UpdateAsync(inquiry, cancellationToken);

            await _notificationService.SendNotificationAsync(
                inquiry.StudentId,
                "Tutor Replied to Your Inquiry",
                "The tutor has replied to your inquiry.",
                NotificationType.NewMessage,
                $"/inquiries/{inquiry.Id}",
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var student = await _userRepository.GetByIdAsync(inquiry.StudentId, cancellationToken);
            var tutor = await _userRepository.GetByIdAsync(inquiry.TutorId, cancellationToken);

            return Result<TutorInquiryDto>.SuccessResult(new TutorInquiryDto
            {
                Id = inquiry.Id,
                StudentId = inquiry.StudentId,
                StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "",
                TutorId = inquiry.TutorId,
                TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "",
                SubjectId = inquiry.SubjectId,
                Message = inquiry.Message,
                TutorReply = inquiry.TutorReply,
                RepliedAt = inquiry.RepliedAt,
                Status = inquiry.Status,
                CreatedAt = inquiry.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error replying to inquiry");
            return Result<TutorInquiryDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class CloseInquiryCommandHandler : IRequestHandler<CloseInquiryCommand, Result>
{
    private readonly IRepository<TutorInquiry> _inquiryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CloseInquiryCommandHandler> _logger;

    public CloseInquiryCommandHandler(
        IRepository<TutorInquiry> inquiryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<CloseInquiryCommandHandler> logger)
    {
        _inquiryRepository = inquiryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CloseInquiryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var inquiry = await _inquiryRepository.GetByIdAsync(request.InquiryId, cancellationToken);
            if (inquiry == null)
                return Result.FailureResult("NOT_FOUND", "Inquiry not found");

            if (inquiry.StudentId != userId.Value && inquiry.TutorId != userId.Value)
                return Result.FailureResult("FORBIDDEN", "You are not authorized to close this inquiry");

            inquiry.Status = InquiryStatus.Closed;
            inquiry.UpdatedAt = DateTime.UtcNow;

            await _inquiryRepository.UpdateAsync(inquiry, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.SuccessResult("Inquiry closed");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error closing inquiry");
            return Result.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetSentInquiriesQueryHandler : IRequestHandler<GetSentInquiriesQuery, Result<List<TutorInquiryDto>>>
{
    private readonly IRepository<TutorInquiry> _inquiryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetSentInquiriesQueryHandler> _logger;

    public GetSentInquiriesQueryHandler(
        IRepository<TutorInquiry> inquiryRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        ILogger<GetSentInquiriesQueryHandler> logger)
    {
        _inquiryRepository = inquiryRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<TutorInquiryDto>>> Handle(GetSentInquiriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<TutorInquiryDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var inquiries = (await _inquiryRepository.FindAsync(i => i.StudentId == userId.Value, cancellationToken))
                .OrderByDescending(i => i.CreatedAt).ToList();

            var userIds = inquiries.Select(i => i.TutorId).Concat(inquiries.Select(i => i.StudentId)).Distinct().ToList();
            var users = (await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken))
                .ToDictionary(u => u.Id);

            var dtos = inquiries.Select(i =>
            {
                users.TryGetValue(i.StudentId, out var student);
                users.TryGetValue(i.TutorId, out var tutor);
                return new TutorInquiryDto
                {
                    Id = i.Id,
                    StudentId = i.StudentId,
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "",
                    TutorId = i.TutorId,
                    TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "",
                    SubjectId = i.SubjectId,
                    Message = i.Message,
                    TutorReply = i.TutorReply,
                    RepliedAt = i.RepliedAt,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt
                };
            }).ToList();

            return Result<List<TutorInquiryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sent inquiries");
            return Result<List<TutorInquiryDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetReceivedInquiriesQueryHandler : IRequestHandler<GetReceivedInquiriesQuery, Result<List<TutorInquiryDto>>>
{
    private readonly IRepository<TutorInquiry> _inquiryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetReceivedInquiriesQueryHandler> _logger;

    public GetReceivedInquiriesQueryHandler(
        IRepository<TutorInquiry> inquiryRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        ILogger<GetReceivedInquiriesQueryHandler> logger)
    {
        _inquiryRepository = inquiryRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<TutorInquiryDto>>> Handle(GetReceivedInquiriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<TutorInquiryDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var inquiries = (await _inquiryRepository.FindAsync(i => i.TutorId == userId.Value, cancellationToken))
                .OrderByDescending(i => i.CreatedAt).ToList();

            var userIds = inquiries.Select(i => i.StudentId).Concat(inquiries.Select(i => i.TutorId)).Distinct().ToList();
            var users = (await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken))
                .ToDictionary(u => u.Id);

            var dtos = inquiries.Select(i =>
            {
                users.TryGetValue(i.StudentId, out var student);
                users.TryGetValue(i.TutorId, out var tutor);
                return new TutorInquiryDto
                {
                    Id = i.Id,
                    StudentId = i.StudentId,
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}".Trim() : "",
                    TutorId = i.TutorId,
                    TutorName = tutor != null ? $"{tutor.FirstName} {tutor.LastName}".Trim() : "",
                    SubjectId = i.SubjectId,
                    Message = i.Message,
                    TutorReply = i.TutorReply,
                    RepliedAt = i.RepliedAt,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt
                };
            }).ToList();

            return Result<List<TutorInquiryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting received inquiries");
            return Result<List<TutorInquiryDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}
