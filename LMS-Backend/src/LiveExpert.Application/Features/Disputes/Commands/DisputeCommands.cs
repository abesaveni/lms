using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Disputes.Commands;

// Create Dispute Command
public class CreateDisputeCommand : IRequest<Result<Guid>>
{
    public Guid SessionId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, Result<Guid>>
{
    private readonly IRepository<Dispute> _disputeRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDisputeCommandHandler(
        IRepository<Dispute> disputeRepository,
        IRepository<SessionBooking> bookingRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _disputeRepository = disputeRepository;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDisputeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<Guid>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var booking = await _bookingRepository.FirstOrDefaultAsync(
            b => b.SessionId == request.SessionId && b.StudentId == userId.Value, cancellationToken);

        if (booking == null)
            return Result<Guid>.FailureResult("NOT_FOUND", "Session booking not found");

        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            RelatedToId = request.SessionId,
            RelatedToType = "Session",
            RaisedBy = userId.Value,
            Subject = request.Reason,
            Description = request.Description,
            DisputeType = DisputeType.SessionQuality,
            Status = DisputeStatus.Open,
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _disputeRepository.AddAsync(dispute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify admin (get all admin users)
        // TODO: Get all admin users and notify them
        // Note: Admin notification would require getting all admin users and sending notifications

        return Result<Guid>.SuccessResult(dispute.Id);
    }
}

// Get Disputes Query
public class GetDisputesQuery : IRequest<Result<List<DisputeDto>>>
{
}

public class DisputeDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public string RaisedByName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class GetDisputesQueryHandler : IRequestHandler<GetDisputesQuery, Result<List<DisputeDto>>>
{
    private readonly IRepository<Dispute> _disputeRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDisputesQueryHandler(
        IRepository<Dispute> disputeRepository,
        IRepository<Session> sessionRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _disputeRepository = disputeRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<DisputeDto>>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<DisputeDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var disputes = await _disputeRepository.FindAsync(d => d.RaisedBy == userId.Value, cancellationToken);

        var dtos = new List<DisputeDto>();
        foreach (var dispute in disputes)
        {
            var session = dispute.RelatedToId.HasValue 
                ? await _sessionRepository.GetByIdAsync(dispute.RelatedToId.Value, cancellationToken)
                : null;
            var user = await _userRepository.GetByIdAsync(dispute.RaisedBy, cancellationToken);

            dtos.Add(new DisputeDto
            {
                Id = dispute.Id,
                SessionId = dispute.RelatedToId ?? Guid.Empty,
                SessionTitle = session?.Title ?? "Unknown",
                RaisedByName = user?.Username ?? "Unknown",
                Reason = dispute.Subject,
                Description = dispute.Description,
                Status = dispute.Status.ToString(),
                Resolution = dispute.Resolution,
                CreatedAt = dispute.CreatedAt,
                ResolvedAt = dispute.ResolvedAt
            });
        }

        return Result<List<DisputeDto>>.SuccessResult(dtos);
    }
}

// Respond to Dispute Command
public class RespondToDisputeCommand : IRequest<Result<bool>>
{
    public Guid DisputeId { get; set; }
    public string Response { get; set; } = string.Empty;
}

public class RespondToDisputeCommandHandler : IRequestHandler<RespondToDisputeCommand, Result<bool>>
{
    private readonly IRepository<Dispute> _disputeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public RespondToDisputeCommandHandler(
        IRepository<Dispute> disputeRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _disputeRepository = disputeRepository;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RespondToDisputeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<bool>.FailureResult("UNAUTHORIZED", "User not authenticated");

        var dispute = await _disputeRepository.GetByIdAsync(request.DisputeId, cancellationToken);
        if (dispute == null)
            return Result<bool>.FailureResult("NOT_FOUND", "Dispute not found");

        dispute.Resolution = request.Response;
        dispute.Status = DisputeStatus.Resolved;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.AssignedTo = userId.Value;
        dispute.UpdatedAt = DateTime.UtcNow;

        await _disputeRepository.UpdateAsync(dispute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify user
        await _notificationService.SendNotificationAsync(
            dispute.RaisedBy,
            "Dispute Resolved",
            "Your dispute has been resolved");

        return Result<bool>.SuccessResult(true);
    }
}
