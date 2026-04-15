using Hangfire;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly INotificationPreferenceService _preferenceService;
    private readonly INotificationService _notificationService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        INotificationPreferenceService preferenceService,
        INotificationService notificationService,
        IBackgroundJobClient backgroundJobClient,
        ILogger<NotificationDispatcher> logger)
    {
        _preferenceService = preferenceService;
        _notificationService = notificationService;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task SendAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default)
    {
        // In-app: fast (DB write + SignalR push) — keep inline
        if (request.SendInApp)
        {
            var canSend = await _preferenceService.IsChannelEnabledAsync(
                request.UserId,
                request.Category,
                NotificationChannel.InApp,
                request.IsTransactional,
                cancellationToken);

            if (canSend)
            {
                try
                {
                    await _notificationService.SendNotificationAsync(
                        request.UserId,
                        request.Title,
                        request.Message,
                        NotificationType.NewMessage,
                        request.ActionUrl,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "In-app notification failed for user {UserId}", request.UserId);
                }
            }
        }

        // Email: slow + unreliable — enqueue as background job so the API responds immediately
        if (request.SendEmail &&
            !string.IsNullOrWhiteSpace(request.EmailTo) &&
            !string.IsNullOrWhiteSpace(request.EmailSubject) &&
            !string.IsNullOrWhiteSpace(request.EmailBody))
        {
            var canSend = await _preferenceService.IsChannelEnabledAsync(
                request.UserId,
                request.Category,
                NotificationChannel.Email,
                request.IsTransactional,
                cancellationToken);

            if (canSend)
            {
                _backgroundJobClient.Enqueue<NotificationDispatchJob>(
                    job => job.SendEmailAsync(
                        request.EmailTo!,
                        request.EmailSubject!,
                        request.EmailBody!,
                        request.EmailIsHtml));
            }
        }

        // WhatsApp: external API call — enqueue as background job
        if (request.SendWhatsApp && !string.IsNullOrWhiteSpace(request.WhatsAppTo))
        {
            var canSend = await _preferenceService.IsChannelEnabledAsync(
                request.UserId,
                request.Category,
                NotificationChannel.WhatsApp,
                request.IsTransactional,
                cancellationToken);

            if (canSend)
            {
                if (!string.IsNullOrWhiteSpace(request.WhatsAppTemplateName))
                {
                    _backgroundJobClient.Enqueue<NotificationDispatchJob>(
                        job => job.SendWhatsAppTemplateAsync(
                            request.WhatsAppTo!,
                            request.WhatsAppTemplateName!,
                            request.WhatsAppParameters ?? new List<string>()));
                }
                else if (!string.IsNullOrWhiteSpace(request.WhatsAppMessage))
                {
                    _backgroundJobClient.Enqueue<NotificationDispatchJob>(
                        job => job.SendWhatsAppMessageAsync(
                            request.WhatsAppTo!,
                            request.WhatsAppMessage!));
                }
            }
        }
    }
}
