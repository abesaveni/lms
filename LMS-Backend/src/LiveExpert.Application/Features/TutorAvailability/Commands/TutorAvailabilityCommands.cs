using LiveExpert.Application.Common;
using MediatR;

namespace LiveExpert.Application.Features.TutorAvailability.Commands;

// Feature 6: Tutor availability slots

public class AvailabilitySlotDto
{
    public int DayOfWeek { get; set; } // 0=Sunday ... 6=Saturday
    public string StartTime { get; set; } = string.Empty; // "HH:mm"
    public string EndTime { get; set; } = string.Empty;   // "HH:mm"
}

public class SetAvailabilityCommand : IRequest<Result<List<TutorAvailabilityDto>>>
{
    public List<AvailabilitySlotDto> Slots { get; set; } = new();
}

public class DeleteAvailabilitySlotCommand : IRequest<Result>
{
    public Guid SlotId { get; set; }
}

public class GetTutorAvailabilityQuery : IRequest<Result<List<TutorAvailabilityDto>>>
{
    public Guid TutorId { get; set; }
}

public class TutorAvailabilityDto
{
    public Guid Id { get; set; }
    public Guid TutorId { get; set; }
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
