using LiveExpert.Application.Common;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Messages.Commands;

// Send Message Command
public class SendMessageCommand : IRequest<Result<Guid>>
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Domain.Enums.MessageType MessageType { get; set; } = Domain.Enums.MessageType.Text;
}

// Create Conversation Command
public class CreateConversationCommand : IRequest<Result<Guid>>
{
    public Guid OtherUserId { get; set; }
}

// Mark Message as Read Command
public class MarkMessageAsReadCommand : IRequest<Result>
{
    public Guid MessageId { get; set; }
}

// Get Conversations Query
public class GetConversationsQuery : IRequest<Result<PaginatedResult<ConversationDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserImage { get; set; }
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

// Get Messages Query
public class GetMessagesQuery : IRequest<Result<PaginatedResult<MessageDto>>>
{
    public Guid ConversationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string? SenderName { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
