using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

// Feature 6: Tutor availability slots
public class TutorAvailability : BaseEntity
{
    public Guid TutorId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public User Tutor { get; set; } = null!;
}
