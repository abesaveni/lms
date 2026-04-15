using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

// Feature 8: Waitlist for full group sessions
public class SessionWaitlist : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public int Position { get; set; }
    public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;
    public DateTime? NotifiedAt { get; set; }

    // Navigation
    public Session Session { get; set; } = null!;
    public User Student { get; set; } = null!;
}
