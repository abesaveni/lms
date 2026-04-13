using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/admin/chat")]
[ApiController]
public class AdminChatController : ControllerBase
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminChatController(
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendAdminMessage([FromBody] AdminChatMessageRequest request)
    {
        var adminId = _currentUserService.UserId;
        if (!adminId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var recipient = await _userRepository.GetByIdAsync(request.UserId);
        if (recipient == null || recipient.Role == UserRole.Admin)
        {
            return BadRequest(Result.FailureResult("INVALID_USER", "Recipient not found"));
        }

        var conversation = await _conversationRepository.FirstOrDefaultAsync(
            c => (c.User1Id == adminId.Value && c.User2Id == recipient.Id) ||
                 (c.User1Id == recipient.Id && c.User2Id == adminId.Value));

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                User1Id = adminId.Value,
                User2Id = recipient.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _conversationRepository.AddAsync(conversation);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = adminId.Value,
            ReceiverId = recipient.Id,
            Content = request.Message,
            MessageType = MessageType.Text,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result<AdminChatMessageResponse>.SuccessResult(new AdminChatMessageResponse
        {
            ConversationId = conversation.Id,
            MessageId = message.Id
        }));
    }
}

public class AdminChatMessageRequest
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AdminChatMessageResponse
{
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
}
