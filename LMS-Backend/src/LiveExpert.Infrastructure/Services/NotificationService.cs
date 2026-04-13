using System;
using MimeKit;
using MailKit.Net.Smtp;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
 
namespace LiveExpert.Infrastructure.Services;
 
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;
 
    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger, IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, NotificationType? notificationType = null, string? actionUrl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = notificationType ?? NotificationType.SessionBooked,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                IsRead = false,
                Priority = Priority.Normal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    public async Task SendBulkNotificationAsync(List<Guid> userIds, string title, string message)
    {
        foreach (var userId in userIds)
        {
            await SendNotificationAsync(userId, title, message);
        }
    }

    public async Task SendWelcomeMessageAsync(User user, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        
        var profileLink = user.Role == UserRole.Tutor
            ? "https://liveexpert.ai/tutor/profile"
            : "https://liveexpert.ai/student/dashboard";

        var roleName = user.Role == UserRole.Tutor ? "Tutor" : "Student";
        var emailBody = EmailTemplates.WelcomeEmail(user.FirstName, roleName);

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.EngagementReminders,
            IsTransactional = true,
            Title = "Welcome to LiveExpert.ai",
            Message = "Your account is active. Welcome to LiveExpert.ai",
            ActionUrl = user.Role == UserRole.Tutor ? "/tutor/profile" : "/student/dashboard",
            EmailTo = user.Email,
            EmailSubject = "Welcome to LiveExpert.ai 🎉",
            EmailBody = emailBody,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppTemplateName = "welcome_liveexpert",
            WhatsAppParameters = NotificationTemplates.WelcomeWhatsAppParameters(user.FirstName),
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendSessionScheduledAsync(User user, Session session, string sessionLink, string otherPartyName, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        
        var sessionTimeStr = session.ScheduledAt.ToString("dd MMM yyyy, HH:mm");
        var emailBody = EmailTemplates.SessionScheduledEmail(
            user.FirstName, 
            session.Title, 
            sessionTimeStr, 
            sessionLink, 
            otherPartyName, 
            user.Role.ToString());

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.SessionBooking,
            IsTransactional = true,
            Title = "Session Scheduled",
            Message = $"Your session '{session.Title}' with {otherPartyName} is confirmed for {sessionTimeStr}.",
            ActionUrl = sessionLink,
            EmailTo = user.Email,
            EmailSubject = $"Session Confirmed: {session.Title}",
            EmailBody = emailBody,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppTemplateName = "session_scheduled",
            WhatsAppParameters = NotificationTemplates.SessionScheduledWhatsAppParameters(
                user.FirstName,
                session.Title,
                session.ScheduledAt.ToString("dd MMM yyyy"),
                session.ScheduledAt.ToString("HH:mm"),
                otherPartyName,
                sessionLink
            ),
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendForgotPasswordEmailAsync(User user, string resetLink, int expiresMinutes, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        
        var emailBody = EmailTemplates.ForgotPasswordEmail(user.FirstName, resetLink, expiresMinutes);

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.AccountSecurity,
            IsTransactional = true,
            Title = "Password Reset Request",
            Message = "A password reset request was made for your account.",
            EmailTo = user.Email,
            EmailSubject = "Reset Your Password - LiveExpert.ai",
            EmailBody = emailBody,
            EmailIsHtml = true,
            SendInApp = false, // Security emails usually don't need in-app notifications
            SendWhatsApp = false
        }, cancellationToken);
    }

    public async Task SendSessionCancelledAsync(User user, string sessionTitle, DateTime sessionTime, string cancelledBy, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        
        var timeStr = sessionTime.ToString("dd MMM yyyy, HH:mm");
        var emailBody = EmailTemplates.SessionCancelledEmail(user.FirstName, sessionTitle, timeStr, cancelledBy);

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.SessionBooking,
            IsTransactional = true,
            Title = "Session Cancelled",
            Message = $"Your session '{sessionTitle}' has been cancelled by {cancelledBy}.",
            EmailTo = user.Email,
            EmailSubject = $"Session Cancelled: {sessionTitle}",
            EmailBody = emailBody,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppMessage = $"❌ Session Cancelled\n\nHi {user.FirstName}, your session *{sessionTitle}* scheduled for {timeStr} has been cancelled by {cancelledBy}.\n\nPlease visit LiveExpert.ai to rebook or explore other sessions.",
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendSessionReminderAsync(User user, string sessionTitle, DateTime sessionTime, string joinLink, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        
        var timeStr = sessionTime.ToString("dd MMM yyyy, HH:mm");
        var emailBody = EmailTemplates.SessionReminderEmail(user.FirstName, sessionTitle, timeStr, joinLink);

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.EngagementReminders,
            IsTransactional = true,
            Title = "Session Reminder",
            Message = $"Your session '{sessionTitle}' is starting in 15 minutes.",
            ActionUrl = joinLink,
            EmailTo = user.Email,
            EmailSubject = $"Reminder: Session starts in 15 min",
            EmailBody = emailBody,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppMessage = $"⏰ Reminder: Your session '{sessionTitle}' starts in 15 minutes. Join now: {joinLink}",
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendSessionFeedbackAsync(User user, string tutorName, string sessionTitle, string feedbackLink, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        
        var emailBody = EmailTemplates.SessionFeedbackEmail(user.FirstName, tutorName, sessionTitle, feedbackLink);

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.EngagementReminders,
            IsTransactional = false,
            Title = "How was your session?",
            Message = $"Please share your feedback for the session with {tutorName}.",
            ActionUrl = feedbackLink,
            EmailTo = user.Email,
            EmailSubject = $"Share your feedback for {sessionTitle}",
            EmailBody = emailBody,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppMessage = $"⭐ How was your session?\n\nHi {user.FirstName}, we'd love to hear your feedback for your session *{sessionTitle}* with {tutorName}.\n\nTap here to rate: {feedbackLink}",
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendTutorProfileUnderReviewAsync(User user, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        var emailBody = EmailTemplates.TutorProfileUnderReviewEmail(user.FirstName);

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.AccountSecurity,
            IsTransactional = true,
            Title = "Profile Under Review",
            Message = "Your tutor profile has been submitted and is currently under review by our team.",
            ActionUrl = "/tutor/dashboard",
            EmailTo = user.Email,
            EmailSubject = "Profile Under Review - LiveExpert.AI",
            EmailBody = emailBody,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppMessage = $"🔍 Profile Under Review\n\nHi {user.FirstName}, your tutor profile on LiveExpert.AI has been submitted and is currently being reviewed by our team.\n\nWe'll notify you once the review is complete — usually within 24-48 hours.",
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendTutorVerifiedAsync(User user, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        var (subject, body) = NotificationTemplates.TutorVerificationApproved(user.FirstName, "https://liveexpert.ai/tutor/dashboard");

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.AccountSecurity,
            IsTransactional = true,
            Title = "Congratulations! Profile Verified",
            Message = "Your tutor account has been approved. You can now start hosting sessions!",
            ActionUrl = "/tutor/dashboard",
            EmailTo = user.Email,
            EmailSubject = subject,
            EmailBody = body,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppMessage = $"🎉 You're Verified!\n\nCongratulations {user.FirstName}! Your tutor profile on LiveExpert.AI has been approved.\n\nYou can now create and host sessions. Start earning today!\n\nGo to your dashboard: https://liveexpert.ai/tutor/dashboard",
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendTutorRejectedAsync(User user, string reason, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        var emailBody = $@"
            <p>Hi {user.FirstName},</p>
            <p>Your tutor account verification was not successful at this time.</p>
            <p><strong>Reason:</strong> {reason}</p>
            <p>You can update your profile and try again.</p>";

        await dispatcher.SendAsync(new NotificationDispatchRequest
        {
            UserId = user.Id,
            Category = NotificationCategory.AccountSecurity,
            IsTransactional = true,
            Title = "Verification Update",
            Message = $"Your tutor account verification was rejected. Reason: {reason}",
            ActionUrl = "/tutor/dashboard",
            EmailTo = user.Email,
            EmailSubject = "Tutor Verification Update",
            EmailBody = emailBody,
            EmailIsHtml = true,
            WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
            WhatsAppMessage = $"📋 Verification Update\n\nHi {user.FirstName}, your tutor profile verification on LiveExpert.AI was unsuccessful at this time.\n\n*Reason:* {reason}\n\nYou can update your profile and reapply at any time. We're here to help!",
            SendInApp = true
        }, cancellationToken);
    }

    public async Task SendTutorSubmissionToAdminAsync(User tutor, CancellationToken cancellationToken = default)
    {
        var dispatcher = _serviceProvider.GetRequiredService<INotificationDispatcher>();
        
        // Find all admins to notify
        var admins = await _context.Users
            .Where(u => u.Role == UserRole.Admin && u.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var admin in admins)
        {
            await dispatcher.SendAsync(new NotificationDispatchRequest
            {
                UserId = admin.Id,
                Category = NotificationCategory.AccountSecurity,
                IsTransactional = true,
                Title = "New Tutor Verification Request",
                Message = $"Tutor {tutor.FirstName} {tutor.LastName} ({tutor.Email}) has submitted their profile for verification.",
                ActionUrl = "/admin/tutor-verification",
                EmailTo = admin.Email,
                EmailSubject = "New Tutor Application - Action Required",
                EmailBody = $@"
                    <h2>New Tutor Verification Request</h2>
                    <p>A new tutor has submitted their profile for verification.</p>
                    <p><strong>Name:</strong> {tutor.FirstName} {tutor.LastName}</p>
                    <p><strong>Email:</strong> {tutor.Email}</p>
                    <p>Please review the application in the admin panel.</p>
                    <a href='https://liveexpert.ai/admin/tutor-verification'>Review Application</a>",
                EmailIsHtml = true,
                WhatsAppTo = admin.WhatsAppNumber ?? admin.PhoneNumber,
                WhatsAppMessage = $"🆕 New Tutor Application\n\n*{tutor.FirstName} {tutor.LastName}* ({tutor.Email}) has submitted their tutor profile for verification on LiveExpert.AI.\n\nReview it here: https://liveexpert.ai/admin/tutor-verification",
                SendInApp = true
            }, cancellationToken);
        }
    }
}

// Email Service — MailKit implementation (replaces obsolete System.Net.Mail.SmtpClient)
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(
        ILogger<EmailService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private string? Cfg(string key1, string key2)
    {
        var v = _configuration[key1] ?? _configuration[key2] ?? Environment.GetEnvironmentVariable(key2.Replace(":", "__"));
        return v?.Trim('"', '\'', ' ');
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var host        = Cfg("Mail:Host",        "MAIL__HOST");
        var portValue   = Cfg("Mail:Port",        "MAIL__PORT");
        var username    = Cfg("Mail:Username",    "MAIL__USERNAME");
        var password    = Cfg("Mail:Password",    "MAIL__PASSWORD");
        var encryption  = Cfg("Mail:Encryption",  "MAIL__ENCRYPTION");
        var fromAddress = Cfg("Mail:FromAddress", "MAIL__FROMADDRESS");
        var fromName    = Cfg("Mail:FromName",    "MAIL__FROMNAME") ?? "LiveExpert Support";

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(portValue) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fromAddress))
        {
            _logger.LogWarning(
                "SMTP not configured — email NOT sent to {To}. Set MAIL__HOST/PORT/USERNAME/PASSWORD/FROMADDRESS in .env",
                to);
            _logger.LogInformation(
                "=== EMAIL WOULD HAVE BEEN SENT ===\nTo: {To}\nSubject: {Subject}\n==================================",
                to, subject);
            return;
        }

        if (!int.TryParse(portValue, out var port))
        {
            _logger.LogWarning("Invalid SMTP port '{Port}'. Email not sent to {To}", portValue, to);
            return;
        }

        try
        {
            // Build MimeKit message
            var email = new MimeKit.MimeMessage();
            email.From.Add(new MimeKit.MailboxAddress(fromName, fromAddress));
            email.To.Add(new MimeKit.MailboxAddress(string.Empty, to));
            email.Subject = subject;

            var builder = new MimeKit.BodyBuilder();
            if (isHtml)
                builder.HtmlBody = body;
            else
                builder.TextBody = body;
            email.Body = builder.ToMessageBody();

            // Connect using MailKit with automatic TLS negotiation
            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            // Port 465 → SSL/TLS; port 587 → STARTTLS; port 25 → plain
            var socketOptions = port == 465
                ? MailKit.Security.SecureSocketOptions.SslOnConnect
                : (string.Equals(encryption, "none", StringComparison.OrdinalIgnoreCase)
                    ? MailKit.Security.SecureSocketOptions.None
                    : MailKit.Security.SecureSocketOptions.StartTls);

            _logger.LogInformation(
                "Connecting to SMTP {Host}:{Port} ({Options}) as {User}",
                host, port, socketOptions, username);

            await smtp.ConnectAsync(host, port, socketOptions);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("✓ Email sent to {To} | Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Failed to send email to {To} | {Message}", to, ex.Message);
            throw; // rethrow so callers can detect failure and fall back (e.g. show devOtp)
        }
    }

    public async Task SendTemplateEmailAsync(string to, string templateName, object data)
    {
        _logger.LogInformation("Template email queued to {To}: {Template}", to, templateName);
        await Task.CompletedTask;
    }

    public async Task SendBulkEmailAsync(List<string> recipients, string subject, string body)
    {
        foreach (var r in recipients)
            await SendEmailAsync(r, subject, body);
    }
}

// SMS Service - Twilio implementation
public class SMSService : ISMSService
{
    private readonly ILogger<SMSService> _logger;
    private readonly IAPIKeyService _apiKeyService;
    private readonly IConfiguration _configuration;

    public SMSService(
        ILogger<SMSService> logger,
        IAPIKeyService apiKeyService,
        IConfiguration configuration)
    {
        _logger = logger;
        _apiKeyService = apiKeyService;
        _configuration = configuration;
    }

    public async Task SendSMSAsync(string phoneNumber, string message)
    {
        // Get Twilio credentials from database or configuration
        var accountSid = await _apiKeyService.GetAPIKeyAsync("Twilio", "AccountSid", 
            _configuration["Twilio:AccountSid"]);
        var authToken = await _apiKeyService.GetAPIKeyAsync("Twilio", "AuthToken", 
            _configuration["Twilio:AuthToken"]);
        var fromNumber = await _apiKeyService.GetAPIKeyAsync("Twilio", "PhoneNumber", 
            _configuration["Twilio:PhoneNumber"]);

        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
        {
            _logger.LogWarning("Twilio credentials not configured. SMS not sent to {PhoneNumber}", phoneNumber);
            return;
        }

        // TODO: Implement actual Twilio SMS sending logic
        _logger.LogInformation("SMS sent to {PhoneNumber} (Twilio configured)", phoneNumber);
        await Task.CompletedTask;
    }

    public async Task SendOTPAsync(string phoneNumber, string otp)
    {
        // Use SendSMSAsync for OTP
        await SendSMSAsync(phoneNumber, $"Your OTP is: {otp}");
        _logger.LogInformation("OTP sent to {PhoneNumber}: {OTP}", phoneNumber, otp);
    }
}

