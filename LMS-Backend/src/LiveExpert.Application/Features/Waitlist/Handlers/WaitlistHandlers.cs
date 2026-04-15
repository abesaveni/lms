using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Waitlist.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Application.Features.Waitlist.Handlers;

// Feature 8: Waitlist for full group sessions

public class JoinWaitlistCommandHandler : IRequestHandler<JoinWaitlistCommand, Result<WaitlistDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionWaitlist> _waitlistRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JoinWaitlistCommandHandler> _logger;

    public JoinWaitlistCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionWaitlist> waitlistRepository,
        IRepository<SessionBooking> bookingRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<JoinWaitlistCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _waitlistRepository = waitlistRepository;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WaitlistDto>> Handle(JoinWaitlistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<WaitlistDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
                return Result<WaitlistDto>.FailureResult("NOT_FOUND", "Session not found");

            if (session.SessionType != SessionType.Group)
                return Result<WaitlistDto>.FailureResult("VALIDATION_ERROR", "Waitlist is only available for group sessions");

            if (session.CurrentStudents < session.MaxStudents)
                return Result<WaitlistDto>.FailureResult("VALIDATION_ERROR", "Session is not full. You can book directly.");

            // Check for existing booking
            var existingBooking = await _bookingRepository.FirstOrDefaultAsync(
                b => b.SessionId == request.SessionId && b.StudentId == userId.Value && b.BookingStatus != BookingStatus.Cancelled,
                cancellationToken);
            if (existingBooking != null)
                return Result<WaitlistDto>.FailureResult("CONFLICT", "You already have a booking for this session");

            // Check for existing waitlist entry
            var existing = await _waitlistRepository.FirstOrDefaultAsync(
                w => w.SessionId == request.SessionId && w.StudentId == userId.Value && w.Status == WaitlistStatus.Waiting,
                cancellationToken);
            if (existing != null)
                return Result<WaitlistDto>.FailureResult("CONFLICT", "You are already on the waitlist for this session");

            // Determine position
            var waitlistCount = await _waitlistRepository.CountAsync(
                w => w.SessionId == request.SessionId && w.Status == WaitlistStatus.Waiting, cancellationToken);

            var entry = new SessionWaitlist
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                StudentId = userId.Value,
                Position = waitlistCount + 1,
                Status = WaitlistStatus.Waiting,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _waitlistRepository.AddAsync(entry, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<WaitlistDto>.SuccessResult(new WaitlistDto
            {
                Id = entry.Id,
                SessionId = entry.SessionId,
                StudentId = entry.StudentId,
                Position = entry.Position,
                Status = entry.Status,
                NotifiedAt = entry.NotifiedAt,
                CreatedAt = entry.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error joining waitlist");
            return Result<WaitlistDto>.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetWaitlistPositionQueryHandler : IRequestHandler<GetWaitlistPositionQuery, Result<WaitlistDto>>
{
    private readonly IRepository<SessionWaitlist> _waitlistRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetWaitlistPositionQueryHandler> _logger;

    public GetWaitlistPositionQueryHandler(
        IRepository<SessionWaitlist> waitlistRepository,
        ICurrentUserService currentUserService,
        ILogger<GetWaitlistPositionQueryHandler> logger)
    {
        _waitlistRepository = waitlistRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<WaitlistDto>> Handle(GetWaitlistPositionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<WaitlistDto>.FailureResult("UNAUTHORIZED", "User not authenticated");

        try
        {
            var entry = await _waitlistRepository.FirstOrDefaultAsync(
                w => w.SessionId == request.SessionId && w.StudentId == userId.Value,
                cancellationToken);

            if (entry == null)
                return Result<WaitlistDto>.FailureResult("NOT_FOUND", "You are not on the waitlist for this session");

            return Result<WaitlistDto>.SuccessResult(new WaitlistDto
            {
                Id = entry.Id,
                SessionId = entry.SessionId,
                StudentId = entry.StudentId,
                Position = entry.Position,
                Status = entry.Status,
                NotifiedAt = entry.NotifiedAt,
                CreatedAt = entry.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting waitlist position");
            return Result<WaitlistDto>.FailureResult("ERROR", ex.Message);
        }
    }
}
