using LiveExpert.Domain.Common;

namespace LiveExpert.Domain.Entities;

public class TutorFollower : BaseEntity
{
    public Guid TutorId { get; set; }
    public Guid StudentId { get; set; }

    public User Tutor { get; set; } = null!;
    public User Student { get; set; } = null!;
}
