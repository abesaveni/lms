using LiveExpert.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

/// <summary>
/// Hangfire job that delivers a single notification channel (email or WhatsApp).
/// Each method is enqueued separately so failures are retried independently.
/// Hangfire retries on throw; auth errors are swallowed so they don't waste retries.
/// </summary>
public class NotificationDispatchJob
{
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<NotificationDispatchJob> _logger;

    public NotificationDispatchJob(
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        ILogger<NotificationDispatchJob> logger)
    {
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml)
    {
        try
        {
            await _emailService.SendEmailAsync(to, subject, body, isHtml);
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            // Wrong credentials — retrying won't help, swallow so Hangfire doesn't waste attempts
            _logger.LogWarning(ex, "SMTP authentication failed sending to {To} — check MAIL__* env vars", to);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Transient email failure to {To} — Hangfire will retry", to);
            throw; // Let Hangfire apply its retry policy for transient errors
        }
    }

    public async Task SendWhatsAppMessageAsync(string to, string message)
    {
        try
        {
            await _whatsAppService.SendMessageAsync(to, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp message failed to {To} — Hangfire will retry", to);
            throw;
        }
    }

    public async Task SendWhatsAppTemplateAsync(string to, string templateName, List<string> parameters)
    {
        try
        {
            await _whatsAppService.SendTemplateMessageAsync(to, templateName, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp template '{Template}' failed to {To} — Hangfire will retry", templateName, to);
            throw;
        }
    }
}
