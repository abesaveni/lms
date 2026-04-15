using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace LiveExpert.Infrastructure.Services;

public class CalendarConnectionService : ICalendarConnectionService
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IApiSettingService _apiSettingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CalendarConnectionService> _logger;
    private readonly HttpClient _httpClient;

    public CalendarConnectionService(
        ApplicationDbContext context,
        IEncryptionService encryptionService,
        IApiSettingService apiSettingService,
        IConfiguration configuration,
        ILogger<CalendarConnectionService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _encryptionService = encryptionService;
        _apiSettingService = apiSettingService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<string> GetAuthorizationUrlAsync(Guid userId, string frontendRedirectUri, string backendCallbackUrl, CancellationToken cancellationToken = default)
    {
        var clientId = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientId");
        if (string.IsNullOrEmpty(clientId) || clientId.Contains("your-google") || clientId == "")
        {
            clientId = _configuration["GoogleCalendar:ClientId"];
        }

        if (string.IsNullOrEmpty(clientId) || 
            clientId.Contains("your-google") || 
            clientId == "")
        {
            _logger.LogWarning("Google Calendar Client ID not configured. Provide ClientId in Admin Settings or appsettings.json.");
            return null; // Return null so controller can handle it
        }

        var scopes = "https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.events";
        var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}|{frontendRedirectUri}"));

        var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={clientId}&" +
            $"redirect_uri={Uri.EscapeDataString(backendCallbackUrl)}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(scopes)}&" +
            $"access_type=offline&" +
            $"prompt=consent&" +
            $"state={state}";

        return authUrl;
    }

    public async Task<bool> ExchangeCodeForTokensAsync(Guid userId, string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var clientId = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientId");
            if (string.IsNullOrEmpty(clientId) || clientId.Contains("your-google"))
            {
                clientId = _configuration["GoogleCalendar:ClientId"];
            }

            var clientSecret = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientSecret");
            if (string.IsNullOrEmpty(clientSecret) || clientSecret.Contains("your-google"))
            {
                clientSecret = _configuration["GoogleCalendar:ClientSecret"];
            }

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || 
                clientId.Contains("your-google"))
            {
                _logger.LogError("Google Calendar credentials not configured for token exchange.");
                return false;
            }

            var tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to exchange code for tokens: {StatusCode}", response.StatusCode);
                return false;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
            if (tokenResponse == null)
            {
                return false;
            }

            // Get user email from Google
            var userInfo = await GetGoogleUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

            // Store encrypted tokens
            var existingConnection = await _context.UserCalendarConnections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, cancellationToken);

            if (existingConnection != null)
            {
                existingConnection.AccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken);
                existingConnection.RefreshToken = _encryptionService.Encrypt(tokenResponse.RefreshToken ?? string.Empty);
                existingConnection.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                existingConnection.GoogleEmail = userInfo?.Email ?? string.Empty;
                existingConnection.LastRefreshedAt = DateTime.UtcNow;
                existingConnection.UpdatedAt = DateTime.UtcNow;
                _context.UserCalendarConnections.Update(existingConnection);
            }
            else
            {
                var newConnection = new UserCalendarConnection
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Provider = CalendarProvider.Google,
                    AccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken),
                    RefreshToken = _encryptionService.Encrypt(tokenResponse.RefreshToken ?? string.Empty),
                    TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    GoogleEmail = userInfo?.Email ?? string.Empty,
                    IsActive = true,
                    ConnectedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserCalendarConnections.Add(newConnection);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for tokens");
            return false;
        }
    }

    public async Task<string?> GetValidAccessTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await _context.UserCalendarConnections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, cancellationToken);

        if (connection == null)
            return null;

        var accessToken = _encryptionService.Decrypt(connection.AccessToken);
        var refreshToken = _encryptionService.Decrypt(connection.RefreshToken);

        // Check if token is expired or about to expire (within 5 minutes)
        if (connection.TokenExpiry <= DateTime.UtcNow.AddMinutes(5))
        {
            // Refresh token
            accessToken = await RefreshAccessTokenAsync(connection, refreshToken, cancellationToken);
        }

        return accessToken;
    }

    public async Task<bool> IsCalendarConnectedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Google Calendar is no longer required — sessions use Jitsi for video conferencing.
        // Return true so the frontend blocking screen is never shown.
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> RevokeConnectionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await _context.UserCalendarConnections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, cancellationToken);

        if (connection == null)
            return false;

        try
        {
            var accessToken = _encryptionService.Decrypt(connection.AccessToken);
            await _httpClient.PostAsync($"https://oauth2.googleapis.com/revoke?token={accessToken}",
                null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke token at Google, but removing from database");
        }

        connection.IsActive = false;
        connection.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string?> RefreshAccessTokenAsync(UserCalendarConnection connection, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientId");
            if (string.IsNullOrEmpty(clientId) || clientId.Contains("your-google"))
            {
                clientId = _configuration["GoogleCalendar:ClientId"];
            }

            var clientSecret = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientSecret");
            if (string.IsNullOrEmpty(clientSecret) || clientSecret.Contains("your-google"))
            {
                clientSecret = _configuration["GoogleCalendar:ClientSecret"];
            }

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || 
                clientId.Contains("your-google"))
                return null;

            var refreshRequest = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", refreshToken },
                { "grant_type", "refresh_token" }
            };

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(refreshRequest), cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);
            if (tokenResponse == null)
                return null;

            // Update token
            connection.AccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken);
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                connection.RefreshToken = _encryptionService.Encrypt(tokenResponse.RefreshToken);
            }
            connection.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            connection.LastRefreshedAt = DateTime.UtcNow;
            connection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return null;
        }
    }

    private async Task<GoogleUserInfo?> GetGoogleUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GoogleUserInfo>(cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user info");
        }
        return null;
    }

    private class GoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    private class GoogleUserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
