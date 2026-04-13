using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ContactMessageStatus Status { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime? RespondedAt { get; set; }
    public Guid? RespondedBy { get; set; }
}
