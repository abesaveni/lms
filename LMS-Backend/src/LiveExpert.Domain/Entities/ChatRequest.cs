using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class ChatRequest : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TutorId { get; set; }
    public ChatRequestStatus Status { get; set; } = ChatRequestStatus.Pending;
    public Guid? LastActionById { get; set; }
    public DateTime? LastActionAt { get; set; }

    public User Student { get; set; } = null!;
    public User Tutor { get; set; } = null!;
    public User? LastActionBy { get; set; }
}
