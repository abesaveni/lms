using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LiveExpert.API.Middleware;

public class TrialSubscriptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TrialSubscriptionMiddleware> _logger;

    // Routes that are always allowed, regardless of trial/subscription status
    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        // Subscription & trial — must always be accessible to allow payment
        "/api/subscription/status",
        "/api/subscription/create-order",
        "/api/subscription/activate",
        "/api/trial/status",
        // Billing history — students need to see payment records even after trial expires
        "/api/billing",
        // Auth
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/refresh-token",
        "/api/auth/forgot-password",
        "/api/auth/reset-password",
        // Infrastructure
        "/health",
        "/swagger",
        "/hubs",
    };

    public TrialSubscriptionMiddleware(RequestDelegate next, ILogger<TrialSubscriptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        // Only apply to authenticated requests
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Only apply to Students
        var roleClaim = context.User.FindFirst("role")?.Value
            ?? context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.Equals(roleClaim, "student", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Check if this path is always allowed
        var path = context.Request.Path.Value ?? string.Empty;
        if (IsPathAllowed(path))
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await _next(context);
            return;
        }

        // Use a scoped DbContext
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await db.Users.FindAsync(userId);
        if (user == null)
        {
            await _next(context);
            return;
        }

        var now = DateTime.UtcNow;

        // Stamp TrialStartDate on first authenticated request (first login behaviour)
        if (!user.TrialStartDate.HasValue)
        {
            user.TrialStartDate = now;
            user.TrialEndDate = now.AddDays(15);
            await db.SaveChangesAsync();
            _logger.LogInformation("Trial started for student {UserId}, ends {End}", userId, user.TrialEndDate);
        }
        else if (!user.TrialEndDate.HasValue)
        {
            // TrialStartDate exists but TrialEndDate is missing — fix it
            user.TrialEndDate = user.TrialStartDate.Value.AddDays(15);
            await db.SaveChangesAsync();
        }

        // Check if currently subscribed
        var isSubscribed = user.IsSubscribed
            && user.SubscribedUntil.HasValue
            && user.SubscribedUntil.Value > now;

        if (isSubscribed)
        {
            await _next(context);
            return;
        }

        // Check if trial is still active
        var trialEnd = user.TrialEndDate ?? user.TrialStartDate!.Value.AddDays(15);
        if (now <= trialEnd)
        {
            await _next(context);
            return;
        }

        // Trial expired and not subscribed — return 402
        _logger.LogInformation("Access denied for student {UserId} — trial expired on {End}", userId, trialEnd);
        context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            error = "trial_expired",
            message = "Your 15-day free trial has ended. Subscribe to LiveExpert Pro for ₹99/month to continue learning.",
            trialExpiredAt = trialEnd,
            subscriptionRequired = true,
            subscriptionAmount = 99,
            currency = "INR",
            activateUrl = "/api/subscription/create-order"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private static bool IsPathAllowed(string path)
    {
        foreach (var allowed in AllowedPaths)
        {
            if (path.StartsWith(allowed, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

// ---------------------------------------------------------------------------
// Extension method for Program.cs
// ---------------------------------------------------------------------------
public static class TrialSubscriptionMiddlewareExtensions
{
    public static IApplicationBuilder UseTrialSubscription(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TrialSubscriptionMiddleware>();
    }
}
