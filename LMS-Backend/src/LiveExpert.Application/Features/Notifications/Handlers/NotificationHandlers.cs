using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Notifications.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Notifications.Handlers;

// Get Notifications Handler
public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PaginatedResult<NotificationDto>>>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(
        IRepository<Notification> notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedResult<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<PaginatedResult<NotificationDto>>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var allNotifications = await _notificationRepository.FindAsync(
            n => n.UserId == userId.Value && (!request.IsRead.HasValue || n.IsRead == request.IsRead.Value),
            cancellationToken
        );

        var notifications = allNotifications
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType.ToString(),
                Priority = n.Priority.ToString(),
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToList();

        var result = new PaginatedResult<NotificationDto>
        {
            Items = notifications,
            Pagination = new PaginationMetadata
            {
                TotalRecords = allNotifications.Count(),
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(allNotifications.Count() / (double)request.PageSize)
            }
        };

        return Result<PaginatedResult<NotificationDto>>.SuccessResult(result);
    }
}

// Mark Notification as Read Handler
public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        IRepository<Notification> notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification == null)
        {
            return Result.FailureResult("NOT_FOUND", "Notification not found");
        }

        if (notification.UserId != userId.Value)
        {
            return Result.FailureResult("FORBIDDEN", "You can only mark your own notifications as read");
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Notification marked as read");
    }
}

// Mark All Notifications as Read Handler
public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsAsReadCommandHandler(
        IRepository<Notification> notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var unreadNotifications = await _notificationRepository.FindAsync(
            n => n.UserId == userId.Value && !n.IsRead,
            cancellationToken
        );

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult($"{unreadNotifications.Count()} notifications marked as read");
    }
}

// Delete Notification Handler
public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationCommandHandler(
        IRepository<Notification> notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification == null)
        {
            return Result.FailureResult("NOT_FOUND", "Notification not found");
        }

        if (notification.UserId != userId.Value)
        {
            return Result.FailureResult("FORBIDDEN", "You can only delete your own notifications");
        }

        await _notificationRepository.DeleteAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Notification deleted");
    }
}

// Get Unread Count Handler
public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, Result<int>>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadNotificationCountQueryHandler(
        IRepository<Notification> notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<int>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<int>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var count = await _notificationRepository.CountAsync(
            n => n.UserId == userId.Value && !n.IsRead,
            cancellationToken
        );

        return Result<int>.SuccessResult(count);
    }
}
