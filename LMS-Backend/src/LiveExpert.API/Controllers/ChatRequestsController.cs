using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/chat-requests")]
[ApiController]
public class ChatRequestsController : ControllerBase
{
    private readonly IRepository<ChatRequest> _chatRequestRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly INotificationService _notificationService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChatRequestsController(
        IRepository<ChatRequest> chatRequestRepository,
        IRepository<Conversation> conversationRepository,
        IRepository<User> userRepository,
        INotificationService notificationService,
        INotificationDispatcher notificationDispatcher,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _chatRequestRepository = chatRequestRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _notificationDispatcher = notificationDispatcher;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateChatRequestRequest request)
    {
        var studentId = _currentUserService.UserId;
        if (!studentId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var tutor = await _userRepository.GetByIdAsync(request.TutorId);
        if (tutor == null || tutor.Role != UserRole.Tutor)
        {
            return BadRequest(Result.FailureResult("INVALID_TUTOR", "Tutor not found"));
        }

        var existing = await _chatRequestRepository.FirstOrDefaultAsync(
            r => r.StudentId == studentId.Value && r.TutorId == request.TutorId);

        if (existing != null && existing.Status != ChatRequestStatus.Rejected)
        {
            return Ok(Result<ChatRequestDto>.SuccessResult(await BuildDtoAsync(existing)));
        }

        ChatRequest chatRequest;
        if (existing != null)
        {
            existing.Status = ChatRequestStatus.Pending;
            existing.LastActionById = studentId.Value;
            existing.LastActionAt = DateTime.UtcNow;
            await _chatRequestRepository.UpdateAsync(existing);
            chatRequest = existing;
        }
        else
        {
            chatRequest = new ChatRequest
            {
                Id = Guid.NewGuid(),
                StudentId = studentId.Value,
                TutorId = request.TutorId,
                Status = ChatRequestStatus.Pending,
                LastActionById = studentId.Value,
                LastActionAt = DateTime.UtcNow
            };
            await _chatRequestRepository.AddAsync(chatRequest);
        }

        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            tutor.Id,
            "New Chat Request",
            "A student has requested to chat with you.",
            NotificationType.NewMessage,
            "/tutor/inbox");

        var student = await _userRepository.GetByIdAsync(studentId.Value);
        if (student != null)
        {
            var studentName = $"{student.FirstName} {student.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(studentName))
            {
                studentName = student.Username;
            }

            var tutorName = $"{tutor.FirstName} {tutor.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(tutorName))
            {
                tutorName = tutor.Username;
            }

            var chatLink = "https://liveexpert.ai/tutor/inbox";
            await _notificationDispatcher.SendAsync(new NotificationDispatchRequest
            {
                UserId = tutor.Id,
                Category = NotificationCategory.ChatRequests,
                IsTransactional = true,
                Title = "New Chat Request",
                Message = $"{studentName} requested to chat.",
                ActionUrl = "/tutor/inbox",
                WhatsAppTo = tutor.WhatsAppNumber ?? tutor.PhoneNumber,
                WhatsAppMessage = NotificationTemplates.StudentRequestedChatWhatsApp(tutorName, studentName, chatLink),
                SendEmail = false,
                SendInApp = false
            });
        }

        return Ok(Result<ChatRequestDto>.SuccessResult(await BuildDtoAsync(chatRequest)));
    }

    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentRequests()
    {
        var studentId = _currentUserService.UserId;
        if (!studentId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var requests = await _chatRequestRepository.FindAsync(r => r.StudentId == studentId.Value);
        var dtos = new List<ChatRequestDto>();
        foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
        {
            dtos.Add(await BuildDtoAsync(request));
        }

        return Ok(Result<List<ChatRequestDto>>.SuccessResult(dtos));
    }

    [HttpGet("tutor")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> GetTutorRequests()
    {
        var tutorId = _currentUserService.UserId;
        if (!tutorId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var requests = await _chatRequestRepository.FindAsync(r => r.TutorId == tutorId.Value);
        var dtos = new List<ChatRequestDto>();
        foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
        {
            dtos.Add(await BuildDtoAsync(request));
        }

        return Ok(Result<List<ChatRequestDto>>.SuccessResult(dtos));
    }

    [HttpPost("{id}/respond")]
    [Authorize(Roles = "Tutor")]
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondChatRequestRequest request)
    {
        var tutorId = _currentUserService.UserId;
        if (!tutorId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        if (request.Status != ChatRequestStatus.Accepted &&
            request.Status != ChatRequestStatus.Rejected &&
            request.Status != ChatRequestStatus.Hold)
        {
            return BadRequest(Result.FailureResult("INVALID_STATUS", "Invalid status"));
        }

        var chatRequest = await _chatRequestRepository.GetByIdAsync(id);
        if (chatRequest == null || chatRequest.TutorId != tutorId.Value)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Chat request not found"));
        }

        chatRequest.Status = request.Status;
        chatRequest.LastActionById = tutorId.Value;
        chatRequest.LastActionAt = DateTime.UtcNow;
        await _chatRequestRepository.UpdateAsync(chatRequest);

        if (request.Status == ChatRequestStatus.Accepted)
        {
            var existingConversation = await _conversationRepository.FirstOrDefaultAsync(
                c => (c.User1Id == chatRequest.StudentId && c.User2Id == chatRequest.TutorId) ||
                     (c.User1Id == chatRequest.TutorId && c.User2Id == chatRequest.StudentId));

            if (existingConversation == null)
            {
                var conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    User1Id = chatRequest.StudentId,
                    User2Id = chatRequest.TutorId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _conversationRepository.AddAsync(conversation);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            chatRequest.StudentId,
            "Chat Request Updated",
            $"Your chat request was {request.Status}.",
            NotificationType.NewMessage,
            "/student/inbox");

        if (request.Status == ChatRequestStatus.Accepted)
        {
            var tutor = await _userRepository.GetByIdAsync(chatRequest.TutorId);
            var student = await _userRepository.GetByIdAsync(chatRequest.StudentId);
            if (tutor != null && student != null)
            {
                var tutorName = GetDisplayName(tutor, "Tutor");
                var studentName = GetDisplayName(student, "Student");
                var requestLink = "https://liveexpert.ai/student/inbox";
                await _notificationDispatcher.SendAsync(new NotificationDispatchRequest
                {
                    UserId = student.Id,
                    Category = NotificationCategory.ChatRequests,
                    IsTransactional = true,
                    Title = "Chat Request Accepted",
                    Message = $"{tutorName} accepted your chat request.",
                    ActionUrl = "/student/inbox",
                    WhatsAppTo = student.WhatsAppNumber ?? student.PhoneNumber,
                    WhatsAppMessage = NotificationTemplates.TutorAcceptedRequestWhatsApp(studentName, tutorName, requestLink),
                    SendEmail = false,
                    SendInApp = false
                });
            }
        }

        return Ok(Result<ChatRequestDto>.SuccessResult(await BuildDtoAsync(chatRequest)));
    }

    private async Task<ChatRequestDto> BuildDtoAsync(ChatRequest request)
    {
        var tutor = await _userRepository.GetByIdAsync(request.TutorId);
        var student = await _userRepository.GetByIdAsync(request.StudentId);

        var conversation = await _conversationRepository.FirstOrDefaultAsync(
            c => (c.User1Id == request.StudentId && c.User2Id == request.TutorId) ||
                 (c.User1Id == request.TutorId && c.User2Id == request.StudentId));

        return new ChatRequestDto
        {
            Id = request.Id,
            StudentId = request.StudentId,
            StudentName = GetDisplayName(student, "Student"),
            StudentAvatar = student?.ProfileImageUrl,
            TutorId = request.TutorId,
            TutorName = GetDisplayName(tutor, "Tutor"),
            TutorAvatar = tutor?.ProfileImageUrl,
            Status = request.Status.ToString(),
            ConversationId = conversation?.Id,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
    }

    private static string GetDisplayName(User? user, string fallback)
    {
        if (user == null)
        {
            return fallback;
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

        return string.IsNullOrWhiteSpace(user.Email) ? fallback : user.Email;
    }
}

public class CreateChatRequestRequest
{
    public Guid TutorId { get; set; }
}

public class RespondChatRequestRequest
{
    public ChatRequestStatus Status { get; set; }
}

public class ChatRequestDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentAvatar { get; set; }
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string? TutorAvatar { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
