using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class UserNotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationCategory Category { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool WhatsAppEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;

    public User User { get; set; } = null!;
}
