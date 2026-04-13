using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

public class AffindaResumeParserService : IResumeParserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AffindaResumeParserService> _logger;
    private readonly IAPIKeyService _apiKeyService;
    private readonly IConfiguration _configuration;
    private const string BaseUrl = "https://api.affinda.com/v3/";

    public AffindaResumeParserService(
        HttpClient httpClient,
        IAPIKeyService apiKeyService,
        IConfiguration configuration,
        ILogger<AffindaResumeParserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKeyService = apiKeyService;
        _configuration = configuration;
        
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    private async Task<string> GetAPIKeyAsync()
    {
        var directKey = _configuration["Affinda:ApiKeyDirect"];
        if (!string.IsNullOrWhiteSpace(directKey))
        {
            return directKey;
        }

        var configKey = _configuration["Affinda:ApiKey"];
        if (!string.IsNullOrWhiteSpace(configKey))
        {
            return configKey;
        }

        var apiKey = await _apiKeyService.GetAPIKeyAsync("Affinda", "ApiKey", null);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Affinda API key not configured");
        }
        return apiKey;
    }

    private async Task<string?> GetWorkspaceIdAsync()
    {
        // Always use configuration first (most reliable)
        // Database might have outdated/invalid workspace IDs
        var configWorkspaceId = _configuration["Affinda:WorkspaceId"];
        if (!string.IsNullOrWhiteSpace(configWorkspaceId))
        {
            _logger.LogDebug("Using Affinda WorkspaceId from configuration: {WorkspaceId}", configWorkspaceId);
            return configWorkspaceId.Trim();
        }
        
        // Fallback to database only if config is empty
        var dbWorkspaceId = await _apiKeyService.GetAPIKeyAsync("Affinda", "WorkspaceId", null);
        if (!string.IsNullOrWhiteSpace(dbWorkspaceId))
        {
            _logger.LogDebug("Using Affinda WorkspaceId from database: {WorkspaceId}", dbWorkspaceId);
            return dbWorkspaceId.Trim();
        }
        
        // Workspace is optional - Affinda can work without it
        _logger.LogWarning("Affinda WorkspaceId not configured. Resume parsing will proceed without workspace (may use default workspace).");
        return null;
    }

    private async Task<string?> GetDocumentTypeAsync()
    {
        var documentType = await _apiKeyService.GetAPIKeyAsync("Affinda", "DocumentType",
            _configuration["Affinda:DocumentType"]);
        return string.IsNullOrWhiteSpace(documentType) ? null : documentType;
    }

    public async Task<ResumeParseResult> ParseResumeAsync(
        Stream fileStream, 
        string fileName, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current API key (may have been updated by admin)
            var apiKey = (await GetAPIKeyAsync()).Trim();
            var workspaceId = await GetWorkspaceIdAsync();
            var documentType = await GetDocumentTypeAsync();
            
            // Read file stream into memory so we can retry if needed
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            // Try with workspace + document type first
            var response = await TryParseWithWorkspace(memoryStream, fileName, apiKey, workspaceId, documentType, cancellationToken);

            // Retry without workspace if it caused a does_not_exist error
            if (response == null && workspaceId != null)
            {
                _logger.LogInformation("Retrying resume parse without workspace parameter");
                memoryStream.Position = 0;
                response = await TryParseWithWorkspace(memoryStream, fileName, apiKey, null, documentType, cancellationToken);
            }

            // Retry without document type if it caused a does_not_exist error
            if (response == null && documentType != null)
            {
                _logger.LogInformation("Retrying resume parse without workspace or document_type parameters");
                memoryStream.Position = 0;
                response = await TryParseWithWorkspace(memoryStream, fileName, apiKey, null, null, cancellationToken);
            }

            if (response == null)
            {
                throw new Exception("Failed to parse resume after retry");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var affindaResponse = JsonSerializer.Deserialize<AffindaResumeResponse>(
                jsonResponse);

            if (affindaResponse?.Data == null)
            {
                throw new Exception("Invalid response from Affinda API");
            }

            return MapToResumeParseResult(affindaResponse.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing resume with Affinda");
            throw;
        }
    }

    private async Task<HttpResponseMessage?> TryParseWithWorkspace(
        MemoryStream fileStream,
        string fileName,
        string apiKey,
        string? workspaceId,
        string? documentType,
        CancellationToken cancellationToken)
    {
        fileStream.Position = 0;
        
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));
        content.Add(fileContent, "file", fileName);
        
        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            content.Add(new StringContent(workspaceId), "workspace");
        }

        if (!string.IsNullOrWhiteSpace(documentType))
        {
            content.Add(new StringContent(documentType), "document_type");
        }
        content.Add(new StringContent("true"), "wait");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.affinda.com/v3/documents")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            var (code, detail) = ExtractAffindaError(error);
            
            // Return null for any "does_not_exist" error when optional params (workspace / document_type)
            // were sent — callers will strip them off one by one and retry.
            if (code == "does_not_exist" && (!string.IsNullOrWhiteSpace(workspaceId) || !string.IsNullOrWhiteSpace(documentType)))
            {
                _logger.LogWarning("Affinda resource does not exist ({Detail}). Retrying with fewer parameters.", detail);
                return null;
            }
            
            _logger.LogError("Affinda API error: {Error}", error);
            throw new AffindaApiException(response.StatusCode, code, detail, error);
        }

        return response;
    }

    private static (string? Code, string? Detail) ExtractAffindaError(string errorBody)
    {
        if (string.IsNullOrWhiteSpace(errorBody))
        {
            return (null, null);
        }

        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            if (doc.RootElement.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Array &&
                errors.GetArrayLength() > 0)
            {
                var first = errors[0];
                var code = first.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;
                var detail = first.TryGetProperty("detail", out var detailProp) ? detailProp.GetString() : null;
                return (code, detail);
            }
        }
        catch
        {
            // Ignore parsing errors and fall back to raw body.
        }

        return (null, errorBody);
    }

    private ResumeParseResult MapToResumeParseResult(AffindaResumeData data)
    {
        var result = new ResumeParseResult
        {
            // Personal Information
            FirstName = data.CandidateName?.Parsed?.FirstName?.Parsed ?? string.Empty,
            LastName = data.CandidateName?.Parsed?.FamilyName?.Parsed ?? string.Empty,
            FullName = data.CandidateName?.Raw ?? string.Empty,
            Email = data.Emails?.FirstOrDefault()?.Parsed ?? string.Empty,
            PhoneNumber = data.PhoneNumbers?.FirstOrDefault()?.Parsed?.FormattedNumber ?? 
                          data.PhoneNumbers?.FirstOrDefault()?.Parsed?.RawText ?? 
                          data.PhoneNumbers?.FirstOrDefault()?.Raw ?? string.Empty,
            
            // Professional Information
            Headline = data.Objective?.Parsed ?? string.Empty,
            Bio = data.Summary?.Parsed ?? string.Empty,
            
            // Experience
            TotalExperience = (int)(data.TotalYearsExperience ?? 0),
            
            // Education
            HighestEducation = data.Education?.FirstOrDefault()?.Parsed?.Degree?.Raw ?? string.Empty,
            University = data.Education?.FirstOrDefault()?.Parsed?.Organization?.Raw ?? string.Empty,
            
            // Skills
            Skills = data.Skills?.Select(s => s.Parsed?.Name ?? s.Raw ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>(),
            Languages = data.Languages?.Select(l => l.Parsed?.Name ?? l.Raw ?? string.Empty)
                        .Where(l => !string.IsNullOrEmpty(l)).ToList() ?? new List<string>(),
            
            // Certifications
            Certifications = new List<string>(), // Map if exists in v3 schema
            
            // Work Experience Details
            WorkExperience = data.WorkExperience?.Select(w => new WorkExperienceDto
            {
                JobTitle = w.Parsed?.JobTitle?.Raw ?? string.Empty,
                Company = w.Parsed?.Organization?.Raw ?? string.Empty,
                StartDate = w.Parsed?.Dates?.Parsed?.StartDate,
                EndDate = w.Parsed?.Dates?.Parsed?.EndDate,
                Description = w.Parsed?.JobDescription?.Raw ?? string.Empty,
                IsCurrent = w.Parsed?.Dates?.Parsed?.IsCurrent ?? (w.Parsed?.Dates?.Parsed?.EndDate == null)
            }).ToList() ?? new List<WorkExperienceDto>(),
            
            // Education Details
            EducationHistory = data.Education?.Select(e => new EducationDto
            {
                Degree = e.Parsed?.Degree?.Raw ?? string.Empty,
                Institution = e.Parsed?.Organization?.Raw ?? string.Empty,
                FieldOfStudy = string.Empty, // Map if exists
                StartDate = e.Parsed?.Dates?.Parsed?.StartDate,
                EndDate = e.Parsed?.Dates?.Parsed?.EndDate,
                Grade = e.Parsed?.Grade?.Raw ?? string.Empty
            }).ToList() ?? new List<EducationDto>(),
            
            // Location
            Location = data.Location?.Parsed?.Formatted ?? data.Location?.Raw ?? string.Empty,
            City = data.Location?.Parsed?.City ?? string.Empty,
            Country = data.Location?.Parsed?.Country ?? string.Empty,
            
            // Social Links
            LinkedInUrl = data.Websites?.FirstOrDefault(w => (w.Parsed ?? w.Raw ?? "").Contains("linkedin", StringComparison.OrdinalIgnoreCase))?.Parsed ?? 
                          data.Websites?.FirstOrDefault(w => (w.Parsed ?? w.Raw ?? "").Contains("linkedin", StringComparison.OrdinalIgnoreCase))?.Raw ?? string.Empty,
            GitHubUrl = data.Websites?.FirstOrDefault(w => (w.Parsed ?? w.Raw ?? "").Contains("github", StringComparison.OrdinalIgnoreCase))?.Parsed ?? 
                        data.Websites?.FirstOrDefault(w => (w.Parsed ?? w.Raw ?? "").Contains("github", StringComparison.OrdinalIgnoreCase))?.Raw ?? string.Empty,
            PortfolioUrl = data.Websites?.FirstOrDefault(w => !(w.Parsed ?? w.Raw ?? "").Contains("linkedin", StringComparison.OrdinalIgnoreCase) 
                                                               && !(w.Parsed ?? w.Raw ?? "").Contains("github", StringComparison.OrdinalIgnoreCase))?.Parsed ?? 
                           data.Websites?.FirstOrDefault(w => !(w.Parsed ?? w.Raw ?? "").Contains("linkedin", StringComparison.OrdinalIgnoreCase) 
                                                               && !(w.Parsed ?? w.Raw ?? "").Contains("github", StringComparison.OrdinalIgnoreCase))?.Raw ?? string.Empty,
            
            // Raw Data for reference
            RawData = JsonSerializer.Serialize(data)
        };

        // Use total years experience if calculated
        result.YearsOfExperience = result.TotalExperience > 0 ? result.TotalExperience : CalculateYearsOfExperience(data.WorkExperience);

        return result;
    }

    private int CalculateYearsOfExperience(List<AffindaField<AffindaWorkExperience>>? workExperience)
    {
        if (workExperience == null || !workExperience.Any())
            return 0;

        var totalMonths = 0;
        foreach (var exp in workExperience)
        {
            var dates = exp.Parsed?.Dates?.Parsed;
            if (dates?.StartDate != null)
            {
                var endDate = dates.EndDate ?? DateTime.Now;
                var months = ((endDate.Year - dates.StartDate.Value.Year) * 12) + 
                           (endDate.Month - dates.StartDate.Value.Month);
                totalMonths += Math.Max(0, months);
            }
        }

        return totalMonths / 12;
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}

// Affinda API Response Models (V3 / NextGen Parser)
public class AffindaResumeResponse
{
    [JsonPropertyName("data")]
    public AffindaResumeData? Data { get; set; }
}

public class AffindaResumeData
{
    [JsonPropertyName("candidateName")]
    public AffindaCandidateName? CandidateName { get; set; }
    
    [JsonPropertyName("email")]
    public List<AffindaField<string>>? Emails { get; set; }
    
    [JsonPropertyName("phoneNumber")]
    public List<AffindaField<AffindaPhoneNumber>>? PhoneNumbers { get; set; }
    
    [JsonPropertyName("website")]
    public List<AffindaField<string>>? Websites { get; set; }
    
    [JsonPropertyName("location")]
    public AffindaField<AffindaLocation>? Location { get; set; }
    
    [JsonPropertyName("objective")]
    public AffindaField<string>? Objective { get; set; }
    
    [JsonPropertyName("summary")]
    public AffindaField<string>? Summary { get; set; }
    
    [JsonPropertyName("totalYearsExperience")]
    public double? TotalYearsExperience { get; set; }
    
    [JsonPropertyName("workExperience")]
    public List<AffindaField<AffindaWorkExperience>>? WorkExperience { get; set; }
    
    [JsonPropertyName("education")]
    public List<AffindaField<AffindaEducation>>? Education { get; set; }
    
    [JsonPropertyName("skill")]
    public List<AffindaField<AffindaSkill>>? Skills { get; set; }
    
    [JsonPropertyName("language")]
    public List<AffindaField<AffindaLanguage>>? Languages { get; set; }
}

public class AffindaField<T>
{
    [JsonPropertyName("raw")]
    public string? Raw { get; set; }
    
    [JsonPropertyName("parsed")]
    public T? Parsed { get; set; }
}

public class AffindaCandidateName
{
    [JsonPropertyName("raw")]
    public string? Raw { get; set; }
    
    [JsonPropertyName("parsed")]
    public AffindaCandidateNameParsed? Parsed { get; set; }
}

public class AffindaCandidateNameParsed
{
    [JsonPropertyName("firstName")]
    public AffindaField<string>? FirstName { get; set; }
    
    [JsonPropertyName("familyName")]
    public AffindaField<string>? FamilyName { get; set; }
}

public class AffindaPhoneNumber
{
    [JsonPropertyName("rawText")]
    public string? RawText { get; set; }
    
    [JsonPropertyName("formattedNumber")]
    public string? FormattedNumber { get; set; }
}

public class AffindaLocation
{
    [JsonPropertyName("formatted")]
    public string? Formatted { get; set; }
    
    [JsonPropertyName("city")]
    public string? City { get; set; }
    
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

public class AffindaWorkExperience
{
    [JsonPropertyName("jobTitle")]
    public AffindaField<string>? JobTitle { get; set; }
    
    [JsonPropertyName("organization")]
    public AffindaField<string>? Organization { get; set; }
    
    [JsonPropertyName("jobDescription")]
    public AffindaField<string>? JobDescription { get; set; }
    
    [JsonPropertyName("dates")]
    public AffindaField<AffindaDates>? Dates { get; set; }
}

public class AffindaEducation
{
    [JsonPropertyName("degree")]
    public AffindaField<string>? Degree { get; set; }
    
    [JsonPropertyName("organization")]
    public AffindaField<string>? Organization { get; set; }
    
    [JsonPropertyName("grade")]
    public AffindaField<string>? Grade { get; set; }
    
    [JsonPropertyName("dates")]
    public AffindaField<AffindaDates>? Dates { get; set; }
}

public class AffindaDates
{
    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }
    
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }
    
    [JsonPropertyName("isCurrent")]
    public bool? IsCurrent { get; set; }
}

public class AffindaSkill
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class AffindaLanguage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
