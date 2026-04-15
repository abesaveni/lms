using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

// Feature 7: Auto-cancel pending bookings after 24h
public class BookingAutoExpireService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingAutoExpireService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    public BookingAutoExpireService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingAutoExpireService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingAutoExpireService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredBookingsAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error in BookingAutoExpireService");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("BookingAutoExpireService stopped");
    }

    private async Task ProcessExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var cutoff = DateTime.UtcNow.AddHours(-24);

        var expiredBookings = await dbContext.SessionBookings
            .Include(b => b.Session)
            .Where(b => b.BookingStatus == BookingStatus.Pending && b.CreatedAt <= cutoff)
            .ToListAsync(cancellationToken);

        if (!expiredBookings.Any())
            return;

        _logger.LogInformation("Auto-expiring {Count} pending bookings older than 24h", expiredBookings.Count);

        foreach (var booking in expiredBookings)
        {
            try
            {
                booking.BookingStatus = BookingStatus.Cancelled;
                booking.CancellationReason = "Booking auto-cancelled: not confirmed within 24 hours";
                booking.UpdatedAt = DateTime.UtcNow;

                var session = booking.Session;
                if (session != null && session.CurrentStudents > 0)
                {
                    session.CurrentStudents--;
                    session.UpdatedAt = DateTime.UtcNow;
                }

                await notificationService.SendNotificationAsync(
                    booking.StudentId,
                    "Booking Expired",
                    $"Your booking for '{booking.Session?.Title ?? "session"}' was automatically cancelled as it was not confirmed within 24 hours.",
                    NotificationType.SessionCancelled,
                    $"/sessions/{booking.SessionId}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling expired booking {BookingId}", booking.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Auto-expired {Count} bookings", expiredBookings.Count);
    }
}
