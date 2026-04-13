using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class SystemSetting : BaseEntity
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation Properties
    public User? UpdatedByUser { get; set; }
}

public class APIKey : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty; // Encrypted
    public string Environment { get; set; } = "Production";
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation Properties
    public User? UpdatedByUser { get; set; }
}

public class AdminPermission : BaseEntity
{
    public Guid AdminId { get; set; }
    public string PermissionKey { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public Guid? GrantedBy { get; set; }

    // Navigation Properties
    public User Admin { get; set; } = null!;
    public User? GrantedByUser { get; set; }
}

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValue { get; set; } // JSON
    public string? NewValue { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation Properties
    public User? User { get; set; }
}

public class WhatsAppCampaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TargetAudience TargetAudience { get; set; }
    public string MessageTemplate { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public CampaignStatus Status { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation Properties
    public User Creator { get; set; } = null!;
}
