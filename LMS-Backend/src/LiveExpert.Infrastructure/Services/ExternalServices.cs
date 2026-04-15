using LiveExpert.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace LiveExpert.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IApiSettingService _apiSettingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly HttpClient _httpClient;

    public PaymentService(
        IApiSettingService apiSettingService,
        IConfiguration configuration, 
        ILogger<PaymentService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _apiSettingService = apiSettingService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<(string orderId, string key)> CreateOrderAsync(decimal amount, string currency, Dictionary<string, string> metadata)
    {
        // Get Razorpay credentials from encrypted ApiSettings with fallback to configuration
        var keyId = await _apiSettingService.GetApiSettingAsync("Razorpay", "KeyId")
            ?? _configuration["Razorpay:KeyId"]
            ?? throw new InvalidOperationException("Razorpay Key ID not configured");

        var keySecret = await _apiSettingService.GetApiSettingAsync("Razorpay", "KeySecret")
            ?? _configuration["Razorpay:KeySecret"]
            ?? throw new InvalidOperationException("Razorpay Key Secret not configured");

        // Create Razorpay order via API
        var orderData = new
        {
            amount = (int)(amount * 100), // Convert to paise
            currency = currency,
            receipt = $"rcpt_{Guid.NewGuid().ToString("N").Substring(0, 16)}",
            notes = metadata
        };

        var json = System.Text.Json.JsonSerializer.Serialize(orderData);
        
        // Basic auth for Razorpay
        var authValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        
        // We use a retry policy specifically for DNS or network issues (api.razorpay.com)
        int maxRetries = 3;
        int delayMs = 1000;
        Exception? lastException = null;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/orders");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Attempting to create Razorpay order (Attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);
                
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Razorpay order creation failed (Status: {Status}): {Error}", response.StatusCode, errorContent);
                    throw new InvalidOperationException($"Razorpay API returned {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var orderResponse = System.Text.Json.JsonSerializer.Deserialize<RazorpayOrderResponse>(
                    responseContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (orderResponse == null || string.IsNullOrEmpty(orderResponse.Id))
                {
                    throw new InvalidOperationException("Failed to deserialize Razorpay order response");
                }

                _logger.LogInformation("Razorpay order created successfully: {OrderId}", orderResponse.Id);
                return (orderResponse.Id, keyId);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Network/DNS issue during Razorpay order creation (Attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);
                
                if (i < maxRetries - 1)
                {
                    await Task.Delay(delayMs * (i + 1));
                    continue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Razorpay order creation");
                throw;
            }
        }

        throw new InvalidOperationException($"Razorpay order creation failed after {maxRetries} attempts. Network logic: {lastException?.Message}", lastException);
    }

    private class RazorpayOrderResponse
    {
        public string Id { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Receipt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public async Task<bool> VerifyPaymentSignatureAsync(string orderId, string paymentId, string signature)
    {
        // Get Razorpay secret from encrypted ApiSettings with fallback to configuration
        var secret = await _apiSettingService.GetApiSettingAsync("Razorpay", "KeySecret")
            ?? _configuration["Razorpay:KeySecret"]
            ?? throw new InvalidOperationException("Razorpay Key Secret not configured");

        // Verify signature using HMAC SHA256
        var payload = $"{orderId}|{paymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

        var isValid = string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);

        if (isValid)
        {
            _logger.LogInformation("Payment signature verified for order: {OrderId}", orderId);
        }
        else
        {
            _logger.LogWarning("Payment signature verification failed for order: {OrderId}", orderId);
        }

        return isValid;
    }

    public async Task<string> InitiateRefundAsync(string paymentId, decimal amount)
    {
        // TODO: Implement actual Razorpay refund
        var refundId = $"rfnd_{Guid.NewGuid().ToString("N").Substring(0, 14)}";
        _logger.LogInformation("Refund initiated: {PaymentId} amount: {Amount}", paymentId, amount);

        await Task.CompletedTask;
        return refundId;
    }

    public async Task<string> InitiatePayoutAsync(string accountNumber, string ifsc, decimal amount, string purpose)
    {
        // TODO: Implement actual Razorpay payout
        var payoutId = $"payout_{Guid.NewGuid().ToString("N").Substring(0, 14)}";
        _logger.LogInformation("Payout initiated to account: {Account} amount: {Amount}", accountNumber, amount);

        await Task.CompletedTask;
        return payoutId;
    }
}

// File Storage Service
public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try 
        {
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsPath = Path.Combine(wwwrootPath, "uploads");
            
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            // Return relative URL that can be served via UseStaticFiles
            var fileUrl = $"/uploads/{uniqueFileName}";
            _logger.LogInformation("File uploaded locally: {FileName} to {Url}", fileName, fileUrl);
            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file locally");
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileUrl)
    {
        // TODO: Implement actual Azure Blob Storage download
        _logger.LogInformation("File downloaded: {Url}", fileUrl);

        await Task.CompletedTask;
        return new MemoryStream();
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        // TODO: Implement actual Azure Blob Storage deletion
        _logger.LogInformation("File deleted: {Url}", fileUrl);

        await Task.CompletedTask;
    }

    public async Task<string> GetSignedUrlAsync(string fileUrl, int expiryMinutes = 60)
    {
        // TODO: Implement actual signed URL generation
        var signedUrl = $"{fileUrl}?token={Guid.NewGuid()}&expires={expiryMinutes}";
        _logger.LogInformation("Signed URL generated for: {Url}", fileUrl);

        await Task.CompletedTask;
        return signedUrl;
    }
}

// Calendar Service
public class CalendarService : ICalendarService
{
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(ILogger<CalendarService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetAuthorizationUrlAsync(string provider, string redirectUri)
    {
        // TODO: Implement actual OAuth URL generation
        var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?redirect_uri={redirectUri}";
        _logger.LogInformation("Authorization URL generated for provider: {Provider}", provider);

        await Task.CompletedTask;
        return authUrl;
    }

    public async Task<(string accessToken, string refreshToken)> ExchangeCodeForTokensAsync(string code, string provider)
    {
        // TODO: Implement actual token exchange
        var accessToken = $"access_{Guid.NewGuid()}";
        var refreshToken = $"refresh_{Guid.NewGuid()}";
        _logger.LogInformation("Tokens exchanged for provider: {Provider}", provider);

        await Task.CompletedTask;
        return (accessToken, refreshToken);
    }

    public async Task<string> CreateEventAsync(string accessToken, string provider, CalendarEventDto eventDto)
    {
        // TODO: Implement actual calendar event creation
        var eventId = Guid.NewGuid().ToString();
        _logger.LogInformation("Calendar event created: {Title} at {StartTime}", eventDto.Title, eventDto.StartTime);

        await Task.CompletedTask;
        return eventId;
    }

    public async Task UpdateEventAsync(string accessToken, string provider, string eventId, CalendarEventDto eventDto)
    {
        // TODO: Implement actual calendar event update
        _logger.LogInformation("Calendar event updated: {EventId}", eventId);

        await Task.CompletedTask;
    }

    public async Task DeleteEventAsync(string accessToken, string provider, string eventId)
    {
        // TODO: Implement actual calendar event deletion
        _logger.LogInformation("Calendar event deleted: {EventId}", eventId);

        await Task.CompletedTask;
    }

    public async Task<List<CalendarEventDto>> GetEventsAsync(string accessToken, string provider, DateTime startDate, DateTime endDate)
    {
        // TODO: Implement actual calendar events retrieval
        _logger.LogInformation("Calendar events retrieved for {Provider} from {Start} to {End}", provider, startDate, endDate);

        await Task.CompletedTask;
        return new List<CalendarEventDto>();
    }
}

// Google Calendar Service
public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly ILogger<GoogleCalendarService> _logger;
    private readonly IApiSettingService _apiSettingService;
    private readonly ICalendarConnectionService _calendarConnectionService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GoogleCalendarService(
        ILogger<GoogleCalendarService> logger,
        IApiSettingService apiSettingService,
        ICalendarConnectionService calendarConnectionService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _apiSettingService = apiSettingService;
        _calendarConnectionService = calendarConnectionService;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<string> CreateMeetingLinkAsync(string title, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        // This method should be called with a userId context
        // For now, return a placeholder - actual implementation requires userId
        _logger.LogInformation("Meeting link creation requested: {Title} from {Start} to {End}", title, startTime, endTime);

        // Generate a Jitsi Meet link — no OAuth required, works immediately
        var roomName = $"liveexpert-{Guid.NewGuid().ToString("N").Substring(0, 10)}";
        var meetingLink = $"https://meet.jit.si/{roomName}";

        await Task.CompletedTask;
        return meetingLink;
    }

    /// <summary>
    /// Create Google Calendar event with Meet link for a specific user
    /// </summary>
    public async Task<(string eventId, string meetUrl)> CreateCalendarEventWithMeetAsync(
        Guid userId, 
        string title, 
        string description,
        DateTime startTime, 
        DateTime endTime, 
        CancellationToken cancellationToken = default)
    {
        var accessToken = await _calendarConnectionService.GetValidAccessTokenAsync(userId, cancellationToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException("Google Calendar not connected for this user");
        }

        var eventData = new
        {
            summary = title,
            description = description,
            start = new
            {
                dateTime = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                timeZone = "UTC"
            },
            end = new
            {
                dateTime = endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                timeZone = "UTC"
            },
            conferenceData = new
            {
                createRequest = new
                {
                    requestId = Guid.NewGuid().ToString(),
                    conferenceSolutionKey = new { type = "hangoutsMeet" }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(eventData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsync(
            "https://www.googleapis.com/calendar/v3/calendars/primary/events?conferenceDataVersion=1",
            content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create Google Calendar event: {Error}", errorContent);
            throw new InvalidOperationException("Failed to create Google Calendar event");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventResponse = System.Text.Json.JsonSerializer.Deserialize<GoogleCalendarEventResponse>(
            responseContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (eventResponse == null || string.IsNullOrEmpty(eventResponse.Id))
        {
            throw new InvalidOperationException("Invalid response from Google Calendar API");
        }

        var meetUrl = eventResponse.ConferenceData?.EntryPoints?.FirstOrDefault(e => e.EntryPointType == "video")?.Uri 
            ?? $"https://meet.google.com/{Guid.NewGuid().ToString("N").Substring(0, 12)}";

        _logger.LogInformation("Google Calendar event created: {EventId} with Meet: {MeetUrl}", eventResponse.Id, meetUrl);

        return (eventResponse.Id, meetUrl);
    }

    private class GoogleCalendarEventResponse
    {
        public string Id { get; set; } = string.Empty;
        public ConferenceData? ConferenceData { get; set; }
    }

    private class ConferenceData
    {
        public List<EntryPoint>? EntryPoints { get; set; }
    }

    private class EntryPoint
    {
        public string EntryPointType { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
    }
}

// Cache Service
public class CacheService : ICacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly Dictionary<string, object> _cache = new();

    public CacheService(ILogger<CacheService> logger)
    {
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // TODO: Implement actual Redis get
        if (_cache.TryGetValue(key, out var value))
        {
            _logger.LogInformation("Cache hit: {Key}", key);
            return (T?)value;
        }

        _logger.LogInformation("Cache miss: {Key}", key);
        await Task.CompletedTask;
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        // TODO: Implement actual Redis set
        _cache[key] = value!;
        _logger.LogInformation("Cache set: {Key}", key);

        await Task.CompletedTask;
    }

    public async Task RemoveAsync(string key)
    {
        // TODO: Implement actual Redis remove
        _cache.Remove(key);
        _logger.LogInformation("Cache removed: {Key}", key);

        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        // TODO: Implement actual Redis exists
        var exists = _cache.ContainsKey(key);
        _logger.LogInformation("Cache exists check: {Key} = {Exists}", key, exists);

        await Task.CompletedTask;
        return exists;
    }
}
