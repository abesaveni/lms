using LiveExpert.Application.Common;
using LiveExpert.Application.Features.TutorAvailability.Commands;
using LiveExpert.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using TutorAvailabilityEntity = LiveExpert.Domain.Entities.TutorAvailability;

namespace LiveExpert.Application.Features.TutorAvailability.Handlers;

// Feature 6: Tutor availability slots

public class SetAvailabilityCommandHandler : IRequestHandler<SetAvailabilityCommand, Result<List<TutorAvailabilityDto>>>
{
    private readonly IRepository<TutorAvailabilityEntity> _availabilityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetAvailabilityCommandHandler> _logger;

    public SetAvailabilityCommandHandler(
        IRepository<TutorAvailabilityEntity> availabilityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<SetAvailabilityCommandHandler> logger)
    {
        _availabilityRepository = availabilityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<TutorAvailabilityDto>>> Handle(SetAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<List<TutorAvailabilityDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Remove all existing slots for this tutor
            var existing = (await _availabilityRepository.FindAsync(a => a.TutorId == userId.Value, cancellationToken)).ToList();
            if (existing.Any())
                await _availabilityRepository.DeleteRangeAsync(existing, cancellationToken);

            var newSlots = new List<TutorAvailabilityEntity>();
            foreach (var slot in request.Slots)
            {
                if (!TimeSpan.TryParse(slot.StartTime, out var start) || !TimeSpan.TryParse(slot.EndTime, out var end))
                    return Result<List<TutorAvailabilityDto>>.FailureResult("VALIDATION_ERROR",
                        $"Invalid time format for day {slot.DayOfWeek}. Use HH:mm format.");

                if (end <= start)
                    return Result<List<TutorAvailabilityDto>>.FailureResult("VALIDATION_ERROR",
                        $"EndTime must be after StartTime for day {slot.DayOfWeek}");

                newSlots.Add(new TutorAvailabilityEntity
                {
                    Id = Guid.NewGuid(),
                    TutorId = userId.Value,
                    DayOfWeek = slot.DayOfWeek,
                    StartTime = start,
                    EndTime = end,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _availabilityRepository.AddRangeAsync(newSlots, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var dtos = newSlots.Select(MapToDto).ToList();
            return Result<List<TutorAvailabilityDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error setting tutor availability");
            return Result<List<TutorAvailabilityDto>>.FailureResult("ERROR", ex.Message);
        }
    }

    private static TutorAvailabilityDto MapToDto(TutorAvailabilityEntity a) => new()
    {
        Id = a.Id,
        TutorId = a.TutorId,
        DayOfWeek = a.DayOfWeek,
        StartTime = a.StartTime.ToString(@"hh\:mm"),
        EndTime = a.EndTime.ToString(@"hh\:mm"),
        IsActive = a.IsActive
    };
}

public class DeleteAvailabilitySlotCommandHandler : IRequestHandler<DeleteAvailabilitySlotCommand, Result>
{
    private readonly IRepository<TutorAvailabilityEntity> _availabilityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAvailabilitySlotCommandHandler> _logger;

    public DeleteAvailabilitySlotCommandHandler(
        IRepository<TutorAvailabilityEntity> availabilityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAvailabilitySlotCommandHandler> logger)
    {
        _availabilityRepository = availabilityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAvailabilitySlotCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var slot = await _availabilityRepository.GetByIdAsync(request.SlotId, cancellationToken);
            if (slot == null)
                return Result.FailureResult("NOT_FOUND", "Availability slot not found");

            if (slot.TutorId != userId.Value)
                return Result.FailureResult("FORBIDDEN", "You can only delete your own availability slots");

            await _availabilityRepository.DeleteAsync(slot, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.SuccessResult("Slot deleted");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error deleting availability slot");
            return Result.FailureResult("ERROR", ex.Message);
        }
    }
}

public class GetTutorAvailabilityQueryHandler : IRequestHandler<GetTutorAvailabilityQuery, Result<List<TutorAvailabilityDto>>>
{
    private readonly IRepository<TutorAvailabilityEntity> _availabilityRepository;
    private readonly ILogger<GetTutorAvailabilityQueryHandler> _logger;

    public GetTutorAvailabilityQueryHandler(
        IRepository<TutorAvailabilityEntity> availabilityRepository,
        ILogger<GetTutorAvailabilityQueryHandler> logger)
    {
        _availabilityRepository = availabilityRepository;
        _logger = logger;
    }

    public async Task<Result<List<TutorAvailabilityDto>>> Handle(GetTutorAvailabilityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var slots = (await _availabilityRepository.FindAsync(
                a => a.TutorId == request.TutorId && a.IsActive, cancellationToken))
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .Select(a => new TutorAvailabilityDto
                {
                    Id = a.Id,
                    TutorId = a.TutorId,
                    DayOfWeek = a.DayOfWeek,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    IsActive = a.IsActive
                }).ToList();

            return Result<List<TutorAvailabilityDto>>.SuccessResult(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tutor availability");
            return Result<List<TutorAvailabilityDto>>.FailureResult("ERROR", ex.Message);
        }
    }
}
