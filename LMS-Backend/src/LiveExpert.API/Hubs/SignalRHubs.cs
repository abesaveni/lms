using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;

namespace LiveExpert.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<ChatRequest> _chatRequestRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChatHub(
        IRepository<Message> messageRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<User> userRepository,
        IRepository<ChatRequest> chatRequestRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _chatRequestRepository = chatRequestRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task OnConnectedAsync()
    {
        // Hard block anonymous connections
        if (!Context.User.Identity?.IsAuthenticated ?? true)
        {
            Context.Abort();
            return;
        }

        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return;

        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null) return;

        // Verify user is part of conversation
        if (conversation.User1Id != userId.Value && conversation.User2Id != userId.Value)
            return;

        var otherUserId = conversation.User1Id == userId.Value ? conversation.User2Id : conversation.User1Id;

        var canChat = await CanChatAsync(conversation, userId.Value);
        if (!canChat) return;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = userId.Value,
            ReceiverId = otherUserId,
            Content = content,
            MessageType = Domain.Enums.MessageType.Text,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Update conversation summary
        conversation.LastMessageId = message.Id;
        conversation.LastMessageAt = message.CreatedAt;
        
        // Increment unread count for recipient
        if (conversation.User1Id == otherUserId)
            conversation.User1UnreadCount++;
        else
            conversation.User2UnreadCount++;

        await _messageRepository.AddAsync(message);
        await _conversationRepository.UpdateAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        // Get sender user information
        var senderUser = await _userRepository.GetByIdAsync(userId.Value);
        var senderName = senderUser?.Role == UserRole.Admin
            ? "LiveExpert AI"
            : senderUser?.Username ?? senderUser?.Email ?? "User";

        // Send to recipient
        await Clients.Group($"user_{otherUserId}").SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.ConversationId,
            message.SenderId,
            SenderName = senderName,
            message.Content,
            message.CreatedAt
        });
    }

    public async Task MarkAsRead(Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null) return;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _messageRepository.UpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UserTyping(Guid conversationId)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return;

        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null) return;

        var recipientId = conversation.User1Id == userId.Value ? conversation.User2Id : conversation.User1Id;
        
        await Clients.Group($"user_{recipientId}").SendAsync("UserTyping", new
        {
            conversationId,
            userId = userId.Value
        });
    }

    private async Task<bool> CanChatAsync(Conversation conversation, Guid senderId)
    {
        var sender = await _userRepository.GetByIdAsync(senderId);
        if (sender == null)
        {
            return false;
        }

        var otherUserId = conversation.User1Id == senderId ? conversation.User2Id : conversation.User1Id;
        var otherUser = await _userRepository.GetByIdAsync(otherUserId);
        if (otherUser == null)
        {
            return false;
        }

        if (sender.Role == UserRole.Admin || otherUser.Role == UserRole.Admin)
        {
            return true;
        }

        if ((sender.Role == UserRole.Student && otherUser.Role == UserRole.Tutor) ||
            (sender.Role == UserRole.Tutor && otherUser.Role == UserRole.Student))
        {
            var studentId = sender.Role == UserRole.Student ? sender.Id : otherUser.Id;
            var tutorId = sender.Role == UserRole.Tutor ? sender.Id : otherUser.Id;
            var chatRequest = await _chatRequestRepository.FirstOrDefaultAsync(
                r => r.StudentId == studentId && r.TutorId == tutorId && r.Status == ChatRequestStatus.Accepted);
            return chatRequest != null;
        }

        return false;
    }
}

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async Task OnConnectedAsync()
    {
        // Hard block anonymous connections
        if (!Context.User.Identity?.IsAuthenticated ?? true)
        {
            Context.Abort();
            return;
        }

        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        // This would be called from client to mark notification as read
        await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
    }
}

[Authorize]
public class SessionHub : Hub
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly ICurrentUserService _currentUserService;

    public SessionHub(IRepository<Session> sessionRepository, ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
    }

    public async Task JoinSession(Guid sessionId)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        
        await Clients.Group($"session_{sessionId}").SendAsync("UserJoined", new
        {
            userId = userId.Value,
            joinedAt = DateTime.UtcNow
        });
    }

    public async Task LeaveSession(Guid sessionId)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        
        await Clients.Group($"session_{sessionId}").SendAsync("UserLeft", new
        {
            userId = userId.Value,
            leftAt = DateTime.UtcNow
        });
    }

    public async Task SendSessionMessage(Guid sessionId, string message)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return;

        await Clients.Group($"session_{sessionId}").SendAsync("SessionMessage", new
        {
            userId = userId.Value,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task UpdateSessionStatus(Guid sessionId, string status)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue) return;

        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null || session.TutorId != userId.Value) return;

        await Clients.Group($"session_{sessionId}").SendAsync("SessionStatusUpdated", new
        {
            sessionId,
            status,
            updatedAt = DateTime.UtcNow
        });
    }
}
