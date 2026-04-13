using LiveExpert.Application.Common;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Notifications.Commands;

// Get Notifications Query
public class GetNotificationsQuery : IRequest<Result<PaginatedResult<NotificationDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsRead { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Mark Notification as Read Command
public class MarkNotificationAsReadCommand : IRequest<Result>
{
    public Guid NotificationId { get; set; }
}

// Mark All Notifications as Read Command
public class MarkAllNotificationsAsReadCommand : IRequest<Result>
{
    // Uses current user from context
}

// Delete Notification Command
public class DeleteNotificationCommand : IRequest<Result>
{
    public Guid NotificationId { get; set; }
}

// Get Unread Count Query
public class GetUnreadNotificationCountQuery : IRequest<Result<int>>
{
    // Uses current user from context
}
