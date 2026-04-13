using LiveExpert.Application.Features.Messages.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class MessagesController : BaseController
{
    public MessagesController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all conversations for current user
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetConversationsQuery { Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Create or get existing conversation with another user
    /// </summary>
    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get messages in a conversation
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = new GetMessagesQuery 
        { 
            ConversationId = conversationId,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Send a message in a conversation
    /// </summary>
    [HttpPost("conversations/{conversationId}/messages")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request)
    {
        var command = new SendMessageCommand
        {
            ConversationId = conversationId,
            Content = request.Content,
            MessageType = request.MessageType
        };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark a message as read
    /// </summary>
    [HttpPut("messages/{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        var command = new MarkMessageAsReadCommand { MessageId = messageId };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public LiveExpert.Domain.Enums.MessageType MessageType { get; set; } = LiveExpert.Domain.Enums.MessageType.Text;
}
