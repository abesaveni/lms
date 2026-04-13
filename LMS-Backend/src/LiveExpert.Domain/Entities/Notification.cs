using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public Priority Priority { get; set; } = Priority.Normal;
    public string? IconUrl { get; set; }
    public string? Metadata { get; set; } // JSON

    // Navigation Properties
    public User User { get; set; } = null!;
    public ICollection<NotificationChannel> Channels { get; set; } = new List<NotificationChannel>();
}

public class NotificationChannel : BaseEntity
{
    public Guid NotificationId { get; set; }
    public Enums.NotificationChannel ChannelType { get; set; }
    public NotificationChannelStatus Status { get; set; }
    public string? ExternalId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Navigation Properties
    public Notification Notification { get; set; } = null!;
}
