using System.Net;
using System.Text.Json;
using LiveExpert.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LiveExpert.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip exception handling for OPTIONS requests (CORS preflight)
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            
            // Only handle exception if response hasn't started
            if (!context.Response.HasStarted)
            {
                await HandleExceptionAsync(context, ex);
            }
            else
            {
                // If response has started, we can't modify headers, so rethrow
                throw;
            }
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // CRITICAL: Set CORS headers FIRST, before anything else
        // This ensures CORS headers are always present, even on errors
        var origin = context.Request.Headers["Origin"].ToString();
        var allowedOrigins = new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5175", "https://liveexpert.ai" };
        
        // Always set CORS headers if origin is present (for development, allow any localhost)
        if (!string.IsNullOrEmpty(origin))
        {
            if (allowedOrigins.Contains(origin) || origin.Contains("localhost"))
            {
                // Try to set headers even if response has started (some browsers allow this)
                try
                {
                    context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
                    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
                }
                catch
                {
                    // If we can't set headers, continue anyway
                }
            }
        }
        else
        {
            // If no origin header, set default for development
            try
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = "http://localhost:5173";
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
            }
            catch
            {
                // If we can't set headers, continue anyway
            }
        }

        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        var result = Result.FailureResult(
            "INTERNAL_SERVER_ERROR",
            "An error occurred while processing your request. Please try again later.",
            new Dictionary<string, object>
            {
                { "traceId", context.TraceIdentifier }
            }
        );

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly Dictionary<string, (DateTime timestamp, int count)> _requestCounts = new();
    private static readonly object _lock = new();
    private const int MaxRequestsPerMinute = 60;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for OPTIONS requests (CORS preflight), Swagger, and SignalR hubs
        if (context.Request.Method == "OPTIONS" ||
            context.Request.Path.StartsWithSegments("/swagger") || 
            context.Request.Path.StartsWithSegments("/api-docs") ||
            context.Request.Path.StartsWithSegments("/hubs"))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientId(context);
        
        lock (_lock)
        {
            CleanupOldEntries();

            if (_requestCounts.TryGetValue(clientId, out var entry))
            {
                if (entry.timestamp > DateTime.UtcNow.AddMinutes(-1))
                {
                    if (entry.count >= MaxRequestsPerMinute)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                        context.Response.ContentType = "application/json";
                        
                        var result = Result.FailureResult(
                            "RATE_LIMIT_EXCEEDED",
                            "Too many requests. Please try again later."
                        );

                        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        context.Response.WriteAsync(json).Wait();
                        return;
                    }

                    _requestCounts[clientId] = (entry.timestamp, entry.count + 1);
                }
                else
                {
                    _requestCounts[clientId] = (DateTime.UtcNow, 1);
                }
            }
            else
            {
                _requestCounts[clientId] = (DateTime.UtcNow, 1);
            }
        }

        await _next(context);
    }

    private string GetClientId(HttpContext context)
    {
        // Try to get user ID from claims
        var userId = context.User?.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user_{userId}";

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip_{ipAddress}";
    }

    private void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-2);
        var keysToRemove = _requestCounts
            .Where(kvp => kvp.Value.timestamp < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _requestCounts.Remove(key);
        }
    }
}

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var userId = context.User?.FindFirst("userId")?.Value ?? "anonymous";

        try
        {
            await _next(context);

            var duration = DateTime.UtcNow - startTime;
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - User: {UserId} - Duration: {Duration}ms",
                requestMethod,
                requestPath,
                statusCode,
                userId,
                duration.TotalMilliseconds
            );
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            _logger.LogError(
                ex,
                "Request failed: {Method} {Path} - User: {UserId} - Duration: {Duration}ms - Error: {Error}",
                requestMethod,
                requestPath,
                userId,
                duration.TotalMilliseconds,
                ex.Message
            );

            throw;
        }
    }
}

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        _logger.LogInformation(
            "Incoming request: {Method} {Path} {QueryString}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString
        );

        await _next(context);

        // Log response
        _logger.LogInformation(
            "Outgoing response: {StatusCode}",
            context.Response.StatusCode
        );
    }
}

// Extension methods for easy registration
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }

    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditLoggingMiddleware>();
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
