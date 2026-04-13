using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Domain.Entities;

public class GeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GeminiAIService> _logger;

    public GeminiAIService(HttpClient httpClient, IConfiguration config, ApplicationDbContext context, ILogger<GeminiAIService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["GoogleAI:ApiKey"] ?? throw new InvalidOperationException("GoogleAI:ApiKey not configured in appsettings.json");
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateContentAsync(string prompt, int maxOutputTokens = 2048)
    {
        try
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt cannot be empty");

            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("API Key is not configured");

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens,
                    temperature = 0.7
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            // Fastest models first — stop as soon as one succeeds
            var models = new[]
            {
                "gemini-2.0-flash",
                "gemini-2.0-flash-lite",
                "gemini-1.5-flash",
                "gemini-2.5-flash",
                "gemini-1.5-pro"
            };

            var apiVersions = new[] { "v1beta" }; // v1beta supports all current models

            HttpResponseMessage response = null;
            var errors = new List<string>();

            foreach (var version in apiVersions)
            {
                foreach (var model in models)
                {
                    try
                    {
                        _logger.LogInformation("Attempting AI generation with model {Model} ({Version})", model, version);
                        var request = new HttpRequestMessage(
                            HttpMethod.Post,
                            $"https://generativelanguage.googleapis.com/{version}/models/{model}:generateContent?key={_apiKey}"
                        );

                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Successfully generated content using {Model} ({Version})", model, version);
                            goto Success;
                        }
                        
                        var errorBody = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Model {Model} ({Version}) failed with status {Status}: {Error}", model, version, response.StatusCode, errorBody);
                        errors.Add($"{model}-{version} ({response.StatusCode}): {errorBody}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception while attempting model {Model} ({Version})", model, version);
                        errors.Add($"{model}-{version} (Exception): {ex.Message}");
                    }
                }
            }

        Success:
            if (response == null || !response.IsSuccessStatusCode)
            {
                var combinedErrors = string.Join(" | ", errors);
                throw new HttpRequestException($"All gemini models failed. Detailed errors: {combinedErrors}");
            }

            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException("Gemini API returned empty response");

            var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            // Check if response has candidates
            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                // Check for safety reason or other blocks
                if (root.TryGetProperty("promptFeedback", out var feedback))
                {
                    throw new InvalidOperationException($"No candidates. Prompt feedback: {feedback}");
                }
                throw new InvalidOperationException("No candidates in Gemini response");
            }

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var content))
                throw new InvalidOperationException("No content in candidate");

            if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
                throw new InvalidOperationException("No parts in content");

            if (!parts[0].TryGetProperty("text", out var text))
                throw new InvalidOperationException("No text in parts");

            var responseText = text.GetString();

            if (string.IsNullOrEmpty(responseText))
                throw new InvalidOperationException("Text content is empty");

            // Save to DB
            try
            {
                var aiResponse = new AIResponse
                {
                    Prompt = prompt,
                    Response = responseText,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AIResponses.Add(aiResponse);
                await _context.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogWarning(dbEx, "Failed to save AI response to database, but returning content to user.");
            }

            return responseText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Final failure in AI content generation");
            throw new ApplicationException($"Error generating AI content: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListAvailableModelsAsync()
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}"
            );

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(result);
            var models = new List<string>();

            if (doc.RootElement.TryGetProperty("models", out var modelsList))
            {
                foreach (var model in modelsList.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var name))
                    {
                        models.Add(name.GetString() ?? "");
                    }
                }
            }

            return models;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error listing models: {ex.Message}", ex);
        }
    }

    public async Task<string> TutorMatchAsync(string subject, string level, string tutorList)
    {
        var prompt = $"Recommend the best tutor.\n\nStudent subject: {subject}\nLevel: {level}\n\nAvailable tutors:\n{tutorList}\n\nReturn best tutor and explanation.";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> StudyPlanAsync(string subject, string goal, string time, string duration)
    {
        var prompt = $"Create a study plan.\n\nSubject: {subject}\nGoal: {goal}\nTime: {time}\nDuration: {duration}\n\nReturn a weekly study plan.";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> SessionSummaryAsync(string transcript)
    {
        var prompt = $"Summarize this tutoring session.\n\nTranscript:\n{transcript}\n\nReturn:\nsummary\nweak areas\nnext steps";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> QuizGeneratorAsync(string topic, string difficulty)
    {
        var prompt = $"Generate 10 MCQ questions.\n\nTopic: {topic}\nDifficulty: {difficulty}\n\nInclude answers.";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> FlashcardsAsync(string topic)
    {
        var prompt = $"Create flashcards.\n\nTopic: {topic}\n\nFormat:\nQuestion\nAnswer";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> HomeworkHelperAsync(string question)
    {
        var prompt = $"Student question:\n\n{question}\n\nProvide a clear explanation.";
        return await GenerateContentAsync(prompt);
    }
}