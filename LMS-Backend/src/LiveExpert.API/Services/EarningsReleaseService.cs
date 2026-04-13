using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.API.Services;

/// <summary>
/// Background service that runs every hour and flips TutorEarnings from
/// Pending → Available once their AvailableAt date has passed (3-day hold).
/// </summary>
public class EarningsReleaseService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EarningsReleaseService> _logger;

    // Run every hour
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public EarningsReleaseService(
        IServiceScopeFactory scopeFactory,
        ILogger<EarningsReleaseService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EarningsReleaseService started.");

        // Run once immediately on startup, then on interval
        while (!stoppingToken.IsCancellationRequested)
        {
            await ReleaseEarningsAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ReleaseEarningsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            var pendingToRelease = await db.TutorEarnings
                .Where(e => e.Status == EarningStatus.Pending
                         && e.AvailableAt.HasValue
                         && e.AvailableAt.Value <= now)
                .ToListAsync(ct);

            if (pendingToRelease.Count == 0) return;

            foreach (var earning in pendingToRelease)
            {
                earning.Status = EarningStatus.Available;
                earning.UpdatedAt = now;
            }

            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "EarningsReleaseService: released {Count} earnings to Available status.",
                pendingToRelease.Count);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "EarningsReleaseService: error during release cycle.");
        }
    }
}
