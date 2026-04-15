using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

/// <summary>
/// Feature 21: Auto-Renewal background service.
/// Runs every 6 hours and:
///   - Sends renewal reminders 3 days before expiry
///   - Marks expired subscriptions as Expired
///   - For AutoRenew=true subscriptions expiring today, sends payment link via notification
/// </summary>
public class SubscriptionRenewalService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionRenewalService> _logger;

    public SubscriptionRenewalService(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionRenewalService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SubscriptionRenewalService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSubscriptionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubscriptionRenewalService error");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task ProcessSubscriptionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var subRepository = scope.ServiceProvider.GetRequiredService<IRepository<StudentSubscription>>();
        var planRepository = scope.ServiceProvider.GetRequiredService<IRepository<SubscriptionPlan>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;
        var reminderThreshold = now.AddDays(3);
        var allActive = await subRepository.FindAsync(
            s => s.Status == SubscriptionStatus.Active, cancellationToken);

        var toProcess = allActive.ToList();

        foreach (var sub in toProcess)
        {
            try
            {
                // ── Mark expired ──────────────────────────────────────────────
                if (sub.EndDate <= now)
                {
                    sub.Status = SubscriptionStatus.Expired;
                    sub.UpdatedAt = now;
                    await subRepository.UpdateAsync(sub, cancellationToken);

                    // Notify student their subscription expired
                    try
                    {
                        await notificationService.SendNotificationAsync(
                            sub.StudentId,
                            "Subscription Expired",
                            "Your subscription has expired. Renew now to continue booking exclusive sessions.",
                            NotificationType.PaymentFailed,
                            null,
                            cancellationToken);
                    }
                    catch { /* do not block */ }

                    continue;
                }

                // ── 3-day renewal reminder ────────────────────────────────────
                if (sub.EndDate <= reminderThreshold && sub.RenewalReminderSentAt == null)
                {
                    var plan = await planRepository.GetByIdAsync(sub.PlanId, cancellationToken);
                    var daysLeft = Math.Max(0, (int)(sub.EndDate - now).TotalDays);
                    var autoRenewNote = sub.AutoRenew
                        ? " Auto-renewal is ON — you will receive a payment link when it renews."
                        : " Auto-renewal is OFF — enable it in your settings to renew automatically.";

                    try
                    {
                        await notificationService.SendNotificationAsync(
                            sub.StudentId,
                            "Subscription Expiring Soon",
                            $"Your {plan?.Name ?? "subscription"} plan expires in {daysLeft} day(s).{autoRenewNote}",
                            NotificationType.SessionReminder,
                            null,
                            cancellationToken);
                    }
                    catch { /* do not block */ }

                    sub.RenewalReminderSentAt = now;
                    sub.UpdatedAt = now;
                    await subRepository.UpdateAsync(sub, cancellationToken);
                }

                // ── Auto-renew: notify with payment link on expiry day ─────────
                if (sub.AutoRenew && sub.EndDate.Date == now.Date && sub.RenewalReminderSentAt?.Date < now.Date)
                {
                    var plan = await planRepository.GetByIdAsync(sub.PlanId, cancellationToken);
                    try
                    {
                        await notificationService.SendNotificationAsync(
                            sub.StudentId,
                            "Time to Renew Your Subscription",
                            $"Your {plan?.Name ?? "subscription"} subscription is due for renewal at ₹{plan?.MonthlyPrice:N0}/month. Visit the app to complete your renewal payment.",
                            NotificationType.PaymentReceived,
                            null,
                            cancellationToken);
                    }
                    catch { /* do not block */ }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription {SubId}", sub.Id);
            }
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving subscription renewal changes");
        }
    }
}
