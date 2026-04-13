using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using LiveExpert.Application.Interfaces;

namespace LiveExpert.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private const string DefaultApiVersion = "v17.0";
        private readonly ILogger<WhatsAppService> _logger;
        private readonly IAPIKeyService _apiKeyService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WhatsAppService(
            ILogger<WhatsAppService> logger,
            IAPIKeyService apiKeyService,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _apiKeyService = apiKeyService;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            // Try multiple environment variable names
            var envNames = new[] { "WHATSAPP__ACCESSTOKEN", "WHATSAPP_ACCESS_TOKEN", "WhatsApp__AccessToken" };
            foreach (var name in envNames)
            {
                var val = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrWhiteSpace(val) && val.Length > 50)
                {
                    var cleaned = val.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");
                    _logger.LogInformation("WhatsApp AccessToken from environment variable '{Name}'. Length: {Length}", name, cleaned.Length);
                    return cleaned;
                }
            }
            
            // Check configuration
            var configToken = _configuration["WhatsApp:AccessToken"];
            if (!string.IsNullOrWhiteSpace(configToken))
            {
                var cleanedToken = configToken.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");
                _logger.LogInformation("WhatsApp AccessToken from config. Length: {Length}", cleanedToken.Length);
                return cleanedToken;
            }
            
            _logger.LogError("WhatsApp AccessToken is missing!");
            return null;
        }

        private async Task<string?> GetPhoneNumberIdAsync()
        {
            var envNames = new[] { "WHATSAPP__PHONENUMBERID", "WHATSAPP_PHONE_NUMBER_ID", "WhatsApp__PhoneNumberId" };
            foreach (var name in envNames)
            {
                var val = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrWhiteSpace(val)) return val.Trim();
            }

            var configPhoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
            if (!string.IsNullOrWhiteSpace(configPhoneNumberId))
            {
                return configPhoneNumberId.Trim();
            }
            
            _logger.LogWarning("WhatsApp PhoneNumberId is missing!");
            return null;
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            var accessToken = await GetAccessTokenAsync();
            var phoneNumberId = await GetPhoneNumberIdAsync();
            var apiUrl = _configuration["WhatsApp:ApiUrl"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                _logger.LogWarning("WhatsApp credentials not configured. Message not sent to {PhoneNumber}", phoneNumber);
                return false;
            }

            var normalizedNumber = NormalizePhoneNumber(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalizedNumber))
            {
                _logger.LogWarning("Invalid WhatsApp phone number: {PhoneNumber}", phoneNumber);
                return false;
            }

            var apiVersion = _configuration["WhatsApp:ApiVersion"] ?? DefaultApiVersion;
            var baseUrl = string.IsNullOrWhiteSpace(apiUrl)
                ? $"https://graph.facebook.com/{apiVersion}"
                : apiUrl.TrimEnd('/');

            if (baseUrl.Contains("graph.facebook.com") && !baseUrl.Contains("/v"))
            {
                baseUrl = $"{baseUrl}/{apiVersion}";
            }

            var url = $"{baseUrl}/{phoneNumberId}/messages";
            _logger.LogInformation("WhatsApp Request URL: {Url}", url);

            var payload = new
            {
                messaging_product = "whatsapp",
                to = normalizedNumber,
                type = "text",
                text = new
                {
                    body = message
                }
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("WhatsApp API error ({StatusCode}): {Body}", response.StatusCode, responseBody);
                    return false;
                }

                _logger.LogInformation("WhatsApp message sent successfully to {PhoneNumber}. Response: {Body}", phoneNumber, responseBody);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp message to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, List<string> parameters)
        {
            var accessToken = await GetAccessTokenAsync();
            var phoneNumberId = await GetPhoneNumberIdAsync();
            var apiUrl = _configuration["WhatsApp:ApiUrl"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                _logger.LogWarning("WhatsApp credentials not configured. Template message not sent to {PhoneNumber}", phoneNumber);
                return false;
            }

            var normalizedNumber = NormalizePhoneNumber(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalizedNumber))
            {
                _logger.LogWarning("Invalid WhatsApp phone number: {PhoneNumber}", phoneNumber);
                return false;
            }

            var apiVersion = _configuration["WhatsApp:ApiVersion"] ?? DefaultApiVersion;
            var baseUrl = string.IsNullOrWhiteSpace(apiUrl)
                ? $"https://graph.facebook.com/{apiVersion}"
                : apiUrl.TrimEnd('/');

            if (baseUrl.Contains("graph.facebook.com") && !baseUrl.Contains("/v"))
            {
                baseUrl = $"{baseUrl}/{apiVersion}";
            }

            var url = $"{baseUrl}/{phoneNumberId}/messages";
            _logger.LogInformation("WhatsApp Request URL: {Url}", url);
            
            var components = parameters != null && parameters.Any()
                ? new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters.Select(p => new { type = "text", text = p }).ToArray()
                    }
                }
                : null;

            var payload = new
            {
                messaging_product = "whatsapp",
                to = normalizedNumber,
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = "en_US" },
                    components
                }
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("WhatsApp template API error ({StatusCode}): {Body}", response.StatusCode, responseBody);
                    return false;
                }

                _logger.LogInformation("WhatsApp template message sent successfully to {PhoneNumber}. Response: {Body}", phoneNumber, responseBody);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send WhatsApp template message to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendOTPAsync(string phoneNumber, string otp)
        {
            var parameters = new List<string> { otp };
            return await SendTemplateMessageAsync(phoneNumber, "otp_template", parameters);
        }

        public async Task<bool> SendBulkMessageAsync(List<string> phoneNumbers, string message)
        {
            var results = new List<bool>();
            foreach (var phoneNumber in phoneNumbers)
            {
                results.Add(await SendMessageAsync(phoneNumber, message));
            }
            return results.All(r => r);
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            var normalized = phoneNumber.Trim();
            bool hasPlus = normalized.StartsWith("+");
            var digits = Regex.Replace(normalized, "\\D", string.Empty);
            
            if (hasPlus && !string.IsNullOrEmpty(digits))
            {
                return "+" + digits;
            }
            
            if (!string.IsNullOrEmpty(digits))
            {
                // Handle Indian 10-digit numbers (very common in this project context)
                if (digits.Length == 10)
                {
                    _logger.LogInformation("Treating 10-digit number {Digits} as Indian number (+91)", digits);
                    return "+91" + digits;
                }

                // Handle already provided country codes
                if (digits.Length >= 10 && digits.Length <= 15)
                {
                    return "+" + digits;
                }
            }
            
            return digits;
        }
    }
}