using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

public class SessionReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public SessionReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SessionReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Reminder Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending session reminders.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Session Reminder Background Service is stopping.");
    }

    private async Task SendRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<IRepository<Session>>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IRepository<SessionBooking>>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<User>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();

        var reminderThreshold = dateTimeService.UtcNow.AddMinutes(15);
        var now = dateTimeService.UtcNow;

        // Find sessions starting in the next 15-20 minutes that haven't been reminded
        var upcomingSessions = await sessionRepository.FindAsync(s => 
            s.ScheduledAt > now && 
            s.ScheduledAt <= reminderThreshold && 
            s.Status == SessionStatus.Scheduled &&
            !s.IsReminderSent, 
            stoppingToken);

        foreach (var session in upcomingSessions)
        {
            // We should ideally track if a reminder was already sent to avoid duplicates
            // For now, we'll just send it. In a real app, we'd add 'IsReminderSent' to the Session or Booking entity.
            
            var bookings = await bookingRepository.FindAsync(b => 
                b.SessionId == session.Id && 
                b.BookingStatus == BookingStatus.Confirmed, 
                stoppingToken);

            var tutor = await userRepository.GetByIdAsync(session.TutorId, stoppingToken);
            var sessionLink = $"https://liveexpert.ai/sessions/{session.Id}";

            if (tutor != null)
            {
                await notificationService.SendSessionReminderAsync(tutor, session.Title, session.ScheduledAt, sessionLink, stoppingToken);
            }

            foreach (var booking in bookings)
            {
                var student = await userRepository.GetByIdAsync(booking.StudentId, stoppingToken);
                if (student != null)
                {
                    await notificationService.SendSessionReminderAsync(student, session.Title, session.ScheduledAt, sessionLink, stoppingToken);
                }
            }
            
            // Mark session as reminded to avoid duplicate emails in next cycle
            session.IsReminderSent = true; 
            await sessionRepository.UpdateAsync(session, stoppingToken);
        }
    }
}
