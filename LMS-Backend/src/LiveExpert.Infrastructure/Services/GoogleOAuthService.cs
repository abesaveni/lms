using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace LiveExpert.Infrastructure.Services;

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IApiSettingService _apiSettingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleOAuthService> _logger;
    private readonly HttpClient _httpClient;

    public GoogleOAuthService(
        ApplicationDbContext context,
        IEncryptionService encryptionService,
        IApiSettingService apiSettingService,
        IConfiguration configuration,
        ILogger<GoogleOAuthService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _encryptionService = encryptionService;
        _apiSettingService = apiSettingService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<string> GetAuthorizationUrlAsync(Guid tutorId, string redirectUri, CancellationToken cancellationToken = default)
    {
        var clientId = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientId") 
            ?? throw new InvalidOperationException("Google Calendar Client ID not configured");

        var scopes = "https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.events";
        var state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{tutorId}|{redirectUri}"));

        var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={clientId}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(scopes)}&" +
            $"access_type=offline&" +
            $"prompt=consent&" +
            $"state={state}";

        return authUrl;
    }

    public async Task<bool> ExchangeCodeForTokensAsync(Guid tutorId, string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var clientId = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientId")
                ?? throw new InvalidOperationException("Google Calendar Client ID not configured");
            var clientSecret = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientSecret")
                ?? throw new InvalidOperationException("Google Calendar Client Secret not configured");

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
            var existingToken = await _context.TutorGoogleTokens
                .FirstOrDefaultAsync(t => t.TutorId == tutorId && t.IsActive, cancellationToken);

            if (existingToken != null)
            {
                existingToken.AccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken);
                existingToken.RefreshToken = _encryptionService.Encrypt(tokenResponse.RefreshToken ?? string.Empty);
                existingToken.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                existingToken.GoogleEmail = userInfo?.Email ?? string.Empty;
                existingToken.LastRefreshedAt = DateTime.UtcNow;
                _context.TutorGoogleTokens.Update(existingToken);
            }
            else
            {
                var newToken = new TutorGoogleTokens
                {
                    Id = Guid.NewGuid(),
                    TutorId = tutorId,
                    AccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken),
                    RefreshToken = _encryptionService.Encrypt(tokenResponse.RefreshToken ?? string.Empty),
                    TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    GoogleEmail = userInfo?.Email ?? string.Empty,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.TutorGoogleTokens.Add(newToken);
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

    public async Task<string?> GetValidAccessTokenAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        var token = await _context.TutorGoogleTokens
            .FirstOrDefaultAsync(t => t.TutorId == tutorId && t.IsActive, cancellationToken);

        if (token == null)
            return null;

        var accessToken = _encryptionService.Decrypt(token.AccessToken);
        var refreshToken = _encryptionService.Decrypt(token.RefreshToken);

        // Check if token is expired or about to expire (within 5 minutes)
        if (token.TokenExpiry <= DateTime.UtcNow.AddMinutes(5))
        {
            // Refresh token
            accessToken = await RefreshAccessTokenAsync(token, refreshToken, cancellationToken);
        }

        return accessToken;
    }

    public async Task<bool> IsGoogleCalendarConnectedAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        var token = await _context.TutorGoogleTokens
            .FirstOrDefaultAsync(t => t.TutorId == tutorId && t.IsActive, cancellationToken);
        return token != null;
    }

    public async Task<bool> RevokeTokensAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        var token = await _context.TutorGoogleTokens
            .FirstOrDefaultAsync(t => t.TutorId == tutorId && t.IsActive, cancellationToken);

        if (token == null)
            return false;

        try
        {
            var accessToken = _encryptionService.Decrypt(token.AccessToken);
            await _httpClient.PostAsync($"https://oauth2.googleapis.com/revoke?token={accessToken}",
                null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke token at Google, but removing from database");
        }

        token.IsActive = false;
        token.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string?> RefreshAccessTokenAsync(TutorGoogleTokens token, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientId");
            var clientSecret = await _apiSettingService.GetApiSettingAsync("GoogleCalendar", "ClientSecret");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
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
            token.AccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken);
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                token.RefreshToken = _encryptionService.Encrypt(tokenResponse.RefreshToken);
            }
            token.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            token.LastRefreshedAt = DateTime.UtcNow;
            token.UpdatedAt = DateTime.UtcNow;

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

// Interface moved to LiveExpert.Application.Interfaces.IApiSettingService
