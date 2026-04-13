using LiveExpert.Application.Features.Notifications.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class NotificationsController : BaseController
{
    public NotificationsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get notifications for current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isRead = null)
    {
        var query = new GetNotificationsQuery 
        { 
            Page = page, 
            PageSize = pageSize,
            IsRead = isRead
        };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var query = new GetUnreadNotificationCountQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkNotificationAsReadCommand { NotificationId = id };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var command = new MarkAllNotificationsAsReadCommand();
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var command = new DeleteNotificationCommand { NotificationId = id };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
