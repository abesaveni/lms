using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

public class Message : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid ConversationId { get; set; }
    public MessageType MessageType { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? DeletedBy { get; set; }
    public Guid? ReplyToMessageId { get; set; }

    // Navigation Properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public Conversation Conversation { get; set; } = null!;
    public Message? ReplyToMessage { get; set; }
}

public class Conversation : BaseEntity
{
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int User1UnreadCount { get; set; }
    public int User2UnreadCount { get; set; }
    public bool User1BlockedUser2 { get; set; }
    public bool User2BlockedUser1 { get; set; }
    public bool User1NotificationsEnabled { get; set; } = true;
    public bool User2NotificationsEnabled { get; set; } = true;

    // Navigation Properties
    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public Message? LastMessage { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
