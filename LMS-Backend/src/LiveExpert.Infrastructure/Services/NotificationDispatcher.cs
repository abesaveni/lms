using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly INotificationPreferenceService _preferenceService;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;

    public NotificationDispatcher(
        INotificationPreferenceService preferenceService,
        INotificationService notificationService,
        IEmailService emailService,
        IWhatsAppService whatsAppService)
    {
        _preferenceService = preferenceService;
        _notificationService = notificationService;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
    }

    public async Task SendAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default)
    {
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
                await _notificationService.SendNotificationAsync(
                    request.UserId,
                    request.Title,
                    request.Message,
                    NotificationType.NewMessage,
                    request.ActionUrl,
                    cancellationToken);
            }
        }

        if (request.SendEmail && !string.IsNullOrWhiteSpace(request.EmailTo) &&
            !string.IsNullOrWhiteSpace(request.EmailSubject) && !string.IsNullOrWhiteSpace(request.EmailBody))
        {
            var canSend = await _preferenceService.IsChannelEnabledAsync(
                request.UserId,
                request.Category,
                NotificationChannel.Email,
                request.IsTransactional,
                cancellationToken);

            if (canSend)
            {
                await _emailService.SendEmailAsync(request.EmailTo!, request.EmailSubject!, request.EmailBody!, request.EmailIsHtml);
            }
        }

        if (request.SendWhatsApp && !string.IsNullOrWhiteSpace(request.WhatsAppTo) &&
            !string.IsNullOrWhiteSpace(request.WhatsAppMessage))
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
                    await _whatsAppService.SendTemplateMessageAsync(
                        request.WhatsAppTo!, 
                        request.WhatsAppTemplateName!, 
                        request.WhatsAppParameters ?? new List<string>());
                }
                else if (!string.IsNullOrWhiteSpace(request.WhatsAppMessage))
                {
                    await _whatsAppService.SendMessageAsync(request.WhatsAppTo!, request.WhatsAppMessage!);
                }
            }
        }
    }
}
