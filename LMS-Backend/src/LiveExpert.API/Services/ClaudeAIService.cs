using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Domain.Entities;

namespace LiveExpert.API.Services;

public class ClaudeAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClaudeAIService> _logger;
    private readonly GeminiAIService _gemini;

    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    /// <summary>Used for high-quality generation (resumes, lesson plans, reports).</summary>
    private const string GenerationModel = "claude-haiku-4-5-20251001";

    /// <summary>Used for real-time chat — fastest Claude model, lowest latency.</summary>
    private const string ChatModel = "claude-haiku-4-5-20251001";

    public ClaudeAIService(HttpClient httpClient, IConfiguration config, ApplicationDbContext context, ILogger<ClaudeAIService> logger, GeminiAIService gemini)
    {
        _httpClient = httpClient;
        _apiKey = config["Anthropic:ApiKey"] ?? string.Empty;
        _context = context;
        _logger = logger;
        _gemini = gemini;

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);
        }
    }

    // Returns true if Claude is likely usable (key present), false → skip straight to Gemini
    private bool HasClaudeKey => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> GenerateContentAsync(string prompt, int maxOutputTokens = 2048)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty");

        // Try Claude first (if key is present)
        if (HasClaudeKey)
        {
            try
            {
                var result = await CallClaudeAsync(prompt);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Claude failed ({Message}), falling back to Gemini.", ex.Message);
            }
        }
        else
        {
            _logger.LogInformation("No Anthropic key configured — using Gemini directly.");
        }

        // Fallback: Gemini
        _logger.LogInformation("Using Gemini as AI provider.");
        return await _gemini.GenerateContentAsync(prompt, maxOutputTokens);
    }

    // Used by LexiChatbot (multi-turn without system prompt)
    public async Task<string> ChatAsync(List<(string Role, string Content)> messages)
    {
        if (messages == null || messages.Count == 0)
            throw new ArgumentException("Messages cannot be empty");

        if (HasClaudeKey)
        {
            try
            {
                return await CallClaudeChatAsync(null, messages);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Claude ChatAsync failed ({Message}), falling back to Gemini.", ex.Message);
            }
        }

        // Gemini fallback: flatten history into a single prompt
        return await _gemini.GenerateContentAsync(FlattenHistory(null, messages));
    }

    // Used by LexiChatbot (multi-turn with system prompt)
    public async Task<string> ChatAsync(string systemPrompt, List<(string Role, string Content)> messages)
    {
        if (messages == null || messages.Count == 0)
            throw new ArgumentException("Messages cannot be empty");

        if (HasClaudeKey)
        {
            try
            {
                return await CallClaudeChatAsync(systemPrompt, messages);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Claude ChatAsync (with system) failed ({Message}), falling back to Gemini.", ex.Message);
            }
        }

        return await _gemini.GenerateContentAsync(FlattenHistory(systemPrompt, messages));
    }

    public async Task<List<string>> ListAvailableModelsAsync()
    {
        return await Task.FromResult(new List<string>
        {
            "claude-opus-4-6",
            "claude-sonnet-4-6",
            "claude-sonnet-4-5",
            "claude-haiku-4-5-20251001"
        });
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<string> CallClaudeAsync(string prompt)
    {
        var requestBody = new
        {
            model = GenerationModel,
            max_tokens = 4096,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _logger.LogInformation("Calling Claude API with model {Model}", GenerationModel);
        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API returned {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Claude API error ({response.StatusCode}): {responseBody}");
        }

        var text = ExtractClaudeText(responseBody);

        try
        {
            _context.AIResponses.Add(new AIResponse { Prompt = prompt, Response = text, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }
        catch (Exception dbEx)
        {
            _logger.LogWarning(dbEx, "Failed to save AI response to DB — returning content anyway");
        }

        return text;
    }

    private async Task<string> CallClaudeChatAsync(string? systemPrompt, List<(string Role, string Content)> messages)
    {
        var messageObjects = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray();

        object requestBody = systemPrompt != null
            ? (object)new { model = ChatModel, max_tokens = 1024, system = systemPrompt, messages = messageObjects }
            : new { model = ChatModel, max_tokens = 1024, messages = messageObjects };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _logger.LogInformation("Calling Claude API for chat, {Count} messages", messages.Count);
        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API returned {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Claude API error ({response.StatusCode}): {responseBody}");
        }

        return ExtractClaudeText(responseBody);
    }

    private static string ExtractClaudeText(string responseBody)
    {
        var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("content", out var contentArray) || contentArray.GetArrayLength() == 0)
            throw new InvalidOperationException("Claude API returned no content");

        var textContent = contentArray[0];
        if (!textContent.TryGetProperty("text", out var textElement))
            throw new InvalidOperationException("Claude API content missing text field");

        return textElement.GetString() ?? string.Empty;
    }

    /// <summary>
    /// Converts a conversation history into a single Gemini-compatible prompt string.
    /// </summary>
    private static string FlattenHistory(string? systemPrompt, List<(string Role, string Content)> messages)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            sb.AppendLine($"[System]: {systemPrompt}\n");

        foreach (var m in messages)
        {
            var label = m.Role == "assistant" ? "Assistant" : "User";
            sb.AppendLine($"{label}: {m.Content}");
        }
        sb.AppendLine("Assistant:");
        return sb.ToString();
    }
}
