using LiveExpert.Domain.Entities;
using LiveExpert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LiveExpert.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<SubscriptionController> _logger;
    private readonly HttpClient _httpClient;

    private const decimal SubscriptionAmountInr = 99m;    // ₹99
    private const int SubscriptionDurationDays = 30;

    public SubscriptionController(
        ApplicationDbContext context,
        IConfiguration config,
        ILogger<SubscriptionController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // -----------------------------------------------------------------------
    // GET /api/subscription/status
    // -----------------------------------------------------------------------
    [HttpGet("subscription/status")]
    public async Task<IActionResult> GetSubscriptionStatus()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { error = "User not found" });

        var isSubscribed = user.IsSubscribed && user.SubscribedUntil.HasValue && user.SubscribedUntil.Value > DateTime.UtcNow;

        return Ok(new
        {
            isSubscribed,
            subscribedUntil = user.SubscribedUntil,
            plan = isSubscribed ? "LiveExpert Pro" : "Free",
            amount = SubscriptionAmountInr,
            currency = "INR"
        });
    }

    // -----------------------------------------------------------------------
    // GET /api/trial/status
    // -----------------------------------------------------------------------
    [HttpGet("trial/status")]
    public async Task<IActionResult> GetTrialStatus()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { error = "User not found" });

        var now = DateTime.UtcNow;
        var isSubscribed = user.IsSubscribed && user.SubscribedUntil.HasValue && user.SubscribedUntil.Value > now;

        bool trialActive = false;
        bool trialExpired = false;
        int daysRemaining = 0;

        if (!user.TrialStartDate.HasValue)
        {
            // Start trial now on first status check (same as middleware behaviour)
            user.TrialStartDate = now;
            user.TrialEndDate = now.AddDays(15);
            await _context.SaveChangesAsync();
        }

        {
            var trialEnd = user.TrialEndDate ?? user.TrialStartDate!.Value.AddDays(15);
            trialActive = now <= trialEnd;
            trialExpired = now > trialEnd;
            daysRemaining = trialActive ? Math.Max(1, (int)Math.Ceiling((trialEnd - now).TotalDays)) : 0;
        }

        return Ok(new
        {
            trialStartDate = user.TrialStartDate,
            trialEndDate = user.TrialEndDate,
            trialActive,
            trialExpired,
            daysRemaining,
            isSubscribed,
            requiresSubscription = trialExpired && !isSubscribed
        });
    }

    // -----------------------------------------------------------------------
    // POST /api/subscription/create-order
    // -----------------------------------------------------------------------
    [HttpPost("subscription/create-order")]
    public async Task<IActionResult> CreateOrder()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var razorpayKeyId = _config["Razorpay:KeyId"];
            var razorpayKeySecret = _config["Razorpay:KeySecret"];

            if (string.IsNullOrEmpty(razorpayKeyId) || string.IsNullOrEmpty(razorpayKeySecret))
                return StatusCode(500, new { error = "Payment gateway not configured" });

            // Amount in paise (₹100 = 10000 paise)
            var amountInPaise = (int)(SubscriptionAmountInr * 100);

            var orderPayload = new
            {
                amount = amountInPaise,
                currency = "INR",
                receipt = $"sub_{userId.ToString("N")[..16]}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100000}",
                notes = new
                {
                    userId = userId.ToString(),
                    purpose = "LiveExpert Pro Subscription"
                }
            };

            var json = JsonSerializer.Serialize(orderPayload);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{razorpayKeyId}:{razorpayKeySecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/orders")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay order creation failed: {Body}", responseBody);
                return StatusCode(500, new { error = "Failed to create payment order", details = responseBody });
            }

            var order = JsonDocument.Parse(responseBody).RootElement;

            return Ok(new
            {
                orderId = order.GetProperty("id").GetString(),
                amount = amountInPaise,
                currency = "INR",
                keyId = razorpayKeyId,
                description = "LiveExpert Pro — 30 Days Full Access"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Razorpay order for user {UserId}", userId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // -----------------------------------------------------------------------
    // POST /api/subscription/dev-activate  [DEV ONLY — no payment required]
    // -----------------------------------------------------------------------
    [HttpPost("subscription/dev-activate")]
    public async Task<IActionResult> DevActivateSubscription()
    {
        var isDev = _config["ASPNETCORE_ENVIRONMENT"] == "Development"
                    || string.IsNullOrEmpty(_config["ASPNETCORE_ENVIRONMENT"]);
        if (!isDev)
            return NotFound(); // silently hidden in production

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { error = "User not found" });

        user.IsSubscribed = true;
        user.SubscribedUntil = DateTime.UtcNow.AddDays(365);
        // Also reset trial so there's no expiry warning
        user.TrialStartDate = DateTime.UtcNow.AddDays(-1);
        user.TrialEndDate = DateTime.UtcNow.AddDays(364);

        await _context.SaveChangesAsync();

        _logger.LogWarning("[DEV] Subscription manually activated for user {UserId} ({Email}), valid until {Until}",
            userId, user.Email, user.SubscribedUntil);

        return Ok(new
        {
            success = true,
            isSubscribed = true,
            subscribedUntil = user.SubscribedUntil,
            message = $"[DEV] Subscription granted until {user.SubscribedUntil:dd MMM yyyy}. No payment was processed."
        });
    }

    // -----------------------------------------------------------------------
    // POST /api/subscription/activate
    // -----------------------------------------------------------------------
    [HttpPost("subscription/activate")]
    public async Task<IActionResult> ActivateSubscription([FromBody] ActivateSubscriptionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            // Verify Razorpay HMAC signature
            var razorpayKeySecret = _config["Razorpay:KeySecret"] ?? string.Empty;
            var payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(razorpayKeySecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

            if (!string.Equals(computedSignature, request.RazorpaySignature, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Razorpay signature for user {UserId}, order {OrderId}", userId, request.RazorpayOrderId);
                return BadRequest(new { error = "Payment verification failed — invalid signature" });
            }

            // Activate subscription
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { error = "User not found" });

            var now = DateTime.UtcNow;
            // If already subscribed and not expired, extend from current end date
            var baseDate = (user.IsSubscribed && user.SubscribedUntil.HasValue && user.SubscribedUntil.Value > now)
                ? user.SubscribedUntil.Value
                : now;

            user.IsSubscribed = true;
            user.SubscribedUntil = baseDate.AddDays(SubscriptionDurationDays);

            // Record platform fee revenue for admin reporting
            _context.PlatformFeePayments.Add(new PlatformFeePayment
            {
                Id = Guid.NewGuid(),
                StudentId = userId,
                Amount = SubscriptionAmountInr,
                GatewayOrderId = request.RazorpayOrderId,
                GatewayPaymentId = request.RazorpayPaymentId,
                PeriodStart = now,
                PeriodEnd = baseDate.AddDays(SubscriptionDurationDays),
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription activated for user {UserId}, valid until {Until}", userId, user.SubscribedUntil);

            return Ok(new
            {
                success = true,
                isSubscribed = true,
                subscribedUntil = user.SubscribedUntil,
                message = $"Welcome to LiveExpert Pro! Your subscription is active until {user.SubscribedUntil:dd MMM yyyy}."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating subscription for user {UserId}", userId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class ActivateSubscriptionRequest
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}
