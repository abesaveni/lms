using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

/// <summary>
/// Runs once per hour and sends a one-time reminder to tutors who registered more than
/// 24 hours ago but have not yet started their profile verification.
/// </summary>
public class TutorVerificationReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TutorVerificationReminderService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public TutorVerificationReminderService(
        IServiceProvider serviceProvider,
        ILogger<TutorVerificationReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tutor Verification Reminder Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending tutor verification reminders.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Tutor Verification Reminder Service is stopping.");
    }

    private async Task SendRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var tutorProfileRepository = scope.ServiceProvider.GetRequiredService<IRepository<TutorProfile>>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<User>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var cutoff = DateTime.UtcNow.AddHours(-24);

        // Tutors registered more than 24h ago who haven't started verification
        var unverifiedProfiles = await tutorProfileRepository.FindAsync(
            tp => tp.VerificationStatus == VerificationStatus.NotStarted
               && tp.CreatedAt <= cutoff
               && !tp.VerificationReminderSent,
            stoppingToken);

        foreach (var profile in unverifiedProfiles)
        {
            var user = await userRepository.GetByIdAsync(profile.UserId, stoppingToken);
            if (user == null) continue;

            try
            {
                var verifyLink = "https://liveexpert.ai/tutor/verification";
                await notificationService.SendNotificationAsync(
                    user.Id,
                    "Complete Your Tutor Verification",
                    "Your profile is not yet verified. Complete your verification to start accepting students and earning from sessions.",
                    NotificationType.SystemAlert,
                    verifyLink,
                    stoppingToken);

                // Mark as reminded so we don't send it again
                profile.VerificationReminderSent = true;
                profile.UpdatedAt = DateTime.UtcNow;
                await tutorProfileRepository.UpdateAsync(profile, stoppingToken);

                _logger.LogInformation("Sent verification reminder to tutor {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send verification reminder to tutor {UserId}", user.Id);
            }
        }
    }
}
