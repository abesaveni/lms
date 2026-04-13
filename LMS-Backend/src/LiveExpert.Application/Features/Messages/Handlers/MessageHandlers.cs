using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Messages.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Messages.Handlers;

// Send Message Handler
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<Guid>>
{
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<ChatRequest> _chatRequestRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
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

    public async Task<Result<Guid>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<Guid>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            return Result<Guid>.FailureResult("NOT_FOUND", "Conversation not found");
        }

        // Verify user is part of conversation
        if (conversation.User1Id != userId.Value && conversation.User2Id != userId.Value)
        {
            return Result<Guid>.FailureResult("FORBIDDEN", "You are not part of this conversation");
        }

        var canChat = await CanChatAsync(conversation, userId.Value, cancellationToken);
        if (!canChat)
        {
            return Result<Guid>.FailureResult("CHAT_NOT_ALLOWED", "Chat is not enabled for this conversation");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = userId.Value,
            Content = request.Content,
            MessageType = request.MessageType,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.SuccessResult(message.Id);
    }

    private async Task<bool> CanChatAsync(Conversation conversation, Guid senderId, CancellationToken cancellationToken)
    {
        var sender = await _userRepository.GetByIdAsync(senderId, cancellationToken);
        if (sender == null)
        {
            return false;
        }

        var otherUserId = conversation.User1Id == senderId ? conversation.User2Id : conversation.User1Id;
        var otherUser = await _userRepository.GetByIdAsync(otherUserId, cancellationToken);
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
                r => r.StudentId == studentId && r.TutorId == tutorId && r.Status == ChatRequestStatus.Accepted,
                cancellationToken);

            return chatRequest != null;
        }

        return false;
    }
}

// Create Conversation Handler
public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, Result<Guid>>
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<ChatRequest> _chatRequestRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversationCommandHandler(
        IRepository<Conversation> conversationRepository,
        IRepository<User> userRepository,
        IRepository<ChatRequest> chatRequestRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _chatRequestRepository = chatRequestRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<Guid>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        // Check if other user exists
        var otherUser = await _userRepository.GetByIdAsync(request.OtherUserId, cancellationToken);
        if (otherUser == null)
        {
            return Result<Guid>.FailureResult("NOT_FOUND", "User not found");
        }

        var currentUser = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (currentUser == null)
        {
            return Result<Guid>.FailureResult("NOT_FOUND", "User not found");
        }

        if (otherUser.Role == UserRole.Admin && currentUser.Role != UserRole.Admin)
        {
            return Result<Guid>.FailureResult("FORBIDDEN", "Only admin can start this conversation");
        }

        if (currentUser.Role != UserRole.Admin && otherUser.Role != UserRole.Admin)
        {
            var isStudentTutorPair = (currentUser.Role == UserRole.Student && otherUser.Role == UserRole.Tutor) ||
                                     (currentUser.Role == UserRole.Tutor && otherUser.Role == UserRole.Student);
            if (!isStudentTutorPair)
            {
                return Result<Guid>.FailureResult("FORBIDDEN", "Conversation not allowed for these roles");
            }

            var studentId = currentUser.Role == UserRole.Student ? currentUser.Id : otherUser.Id;
            var tutorId = currentUser.Role == UserRole.Tutor ? currentUser.Id : otherUser.Id;
            var chatRequest = await _chatRequestRepository.FirstOrDefaultAsync(
                r => r.StudentId == studentId && r.TutorId == tutorId && r.Status == ChatRequestStatus.Accepted,
                cancellationToken);

            if (chatRequest == null)
            {
                return Result<Guid>.FailureResult("CHAT_NOT_ALLOWED", "Chat request not accepted yet");
            }
        }

        // Check if conversation already exists
        var existingConversation = await _conversationRepository.FirstOrDefaultAsync(
            c => (c.User1Id == userId.Value && c.User2Id == request.OtherUserId) ||
                 (c.User1Id == request.OtherUserId && c.User2Id == userId.Value),
            cancellationToken
        );

        if (existingConversation != null)
        {
            return Result<Guid>.SuccessResult(existingConversation.Id);
        }

        // Create new conversation
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = userId.Value,
            User2Id = request.OtherUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.SuccessResult(conversation.Id);
    }
}

// Mark Message as Read Handler
public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, Result>
{
    private readonly IRepository<Message> _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkMessageAsReadCommandHandler(
        IRepository<Message> messageRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var message = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);
        if (message == null)
        {
            return Result.FailureResult("NOT_FOUND", "Message not found");
        }

        // Only recipient can mark as read
        if (message.SenderId == userId.Value)
        {
            return Result.FailureResult("FORBIDDEN", "Cannot mark your own message as read");
        }

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        message.UpdatedAt = DateTime.UtcNow;

        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Message marked as read");
    }
}

// Get Conversations Handler
public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, Result<PaginatedResult<ConversationDto>>>
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationsQueryHandler(
        IRepository<Conversation> conversationRepository,
        IRepository<User> userRepository,
        IRepository<Message> messageRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<PaginatedResult<ConversationDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var allConversations = await _conversationRepository.FindAsync(
            c => c.User1Id == userId.Value || c.User2Id == userId.Value,
            cancellationToken
        );

        var conversations = allConversations
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var conversationDtos = new List<ConversationDto>();

        foreach (var conversation in conversations)
        {
            var otherUserId = conversation.User1Id == userId.Value ? conversation.User2Id : conversation.User1Id;
            var otherUser = await _userRepository.GetByIdAsync(otherUserId, cancellationToken);

            var messages = await _messageRepository.FindAsync(
                m => m.ConversationId == conversation.Id,
                cancellationToken
            );

            var lastMessage = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var unreadCount = messages.Count(m => m.SenderId == otherUserId && !m.IsRead);

            conversationDtos.Add(new ConversationDto
            {
                Id = conversation.Id,
                OtherUserId = otherUserId,
                OtherUserName = GetDisplayName(otherUser),
                OtherUserImage = otherUser?.ProfileImageUrl,
                LastMessageContent = lastMessage?.Content,
                LastMessageAt = lastMessage?.CreatedAt,
                UnreadCount = unreadCount
            });
        }

        var result = new PaginatedResult<ConversationDto>
        {
            Items = conversationDtos,
            Pagination = new PaginationMetadata
            {
                TotalRecords = allConversations.Count(),
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(allConversations.Count() / (double)request.PageSize)
            }
        };

        return Result<PaginatedResult<ConversationDto>>.SuccessResult(result);
    }

    private static string GetDisplayName(User? user)
    {
        if (user == null)
        {
            return "Unknown";
        }

        if (user.Role == UserRole.Admin)
        {
            return "LiveExpert AI";
        }

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            return user.Username;
        }

        return user.Email;
    }
}

// Get Messages Handler
public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, Result<PaginatedResult<MessageDto>>>
{
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMessagesQueryHandler(
        IRepository<Message> messageRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<PaginatedResult<MessageDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            return Result<PaginatedResult<MessageDto>>.FailureResult("NOT_FOUND", "Conversation not found");
        }

        // Verify user is part of conversation
        if (conversation.User1Id != userId.Value && conversation.User2Id != userId.Value)
        {
            return Result<PaginatedResult<MessageDto>>.FailureResult("FORBIDDEN", "You are not part of this conversation");
        }

        var allMessages = await _messageRepository.FindAsync(
            m => m.ConversationId == request.ConversationId,
            cancellationToken
        );

        var pagedMessages = allMessages
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var senderIds = pagedMessages.Select(m => m.SenderId).Distinct().ToList();
        var senderLookup = new Dictionary<Guid, string>();
        foreach (var senderId in senderIds)
        {
            var sender = await _userRepository.GetByIdAsync(senderId, cancellationToken);
            senderLookup[senderId] = GetDisplayName(sender);
        }

        var messages = pagedMessages.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = senderLookup.TryGetValue(m.SenderId, out var name) ? name : "User",
            Content = m.Content,
            MessageType = m.MessageType.ToString(),
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();

        var result = new PaginatedResult<MessageDto>
        {
            Items = messages,
            Pagination = new PaginationMetadata
            {
                TotalRecords = allMessages.Count(),
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(allMessages.Count() / (double)request.PageSize)
            }
        };

        return Result<PaginatedResult<MessageDto>>.SuccessResult(result);
    }

    private static string GetDisplayName(User? user)
    {
        if (user == null)
        {
            return "User";
        }

        if (user.Role == UserRole.Admin)
        {
            return "LiveExpert AI";
        }

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            return user.Username;
        }

        return user.Email;
    }
}
