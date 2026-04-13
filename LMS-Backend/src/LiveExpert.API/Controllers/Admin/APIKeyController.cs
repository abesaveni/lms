using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// API Key Management (Admin only)
/// </summary>
[Route("api/admin/api-keys")]
[ApiController]
[Authorize(Roles = "Admin")]
public class APIKeyController : ControllerBase
{
    private readonly IAPIKeyService _apiKeyService;
    private readonly IRepository<APIKey> _apiKeyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<APIKeyController> _logger;

    public APIKeyController(
        IAPIKeyService apiKeyService,
        IRepository<APIKey> apiKeyRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<APIKeyController> logger)
    {
        _apiKeyService = apiKeyService;
        _apiKeyRepository = apiKeyRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all API keys
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<APIKeyResponse>>), 200)]
    public async Task<IActionResult> GetAllAPIKeys()
    {
        try
        {
            _logger.LogInformation("Getting all API keys");
            var apiKeys = await _apiKeyRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} API keys from database", apiKeys.Count());
            
            var response = apiKeys.Select(k => new APIKeyResponse
            {
                Id = k.Id,
                ServiceName = k.ServiceName ?? string.Empty,
                KeyName = k.KeyName ?? string.Empty,
                Environment = k.Environment ?? "Production",
                IsActive = k.IsActive,
                ExpiresAt = k.ExpiresAt,
                CreatedAt = k.CreatedAt,
                UpdatedAt = k.UpdatedAt
            }).ToList();

            _logger.LogInformation("Successfully mapped {Count} API keys to response", response.Count);
            return Ok(Result<List<APIKeyResponse>>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all API keys. Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, Result<List<APIKeyResponse>>.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}. Inner: {ex.InnerException?.Message}. Stack: {ex.StackTrace}"));
        }
    }

    /// <summary>
    /// Get API keys for a specific service
    /// </summary>
    [HttpGet("service/{serviceName}")]
    [ProducesResponseType(typeof(Result<Dictionary<string, string>>), 200)]
    public async Task<IActionResult> GetServiceAPIKeys(string serviceName)
    {
        try
        {
            var keys = await _apiKeyService.GetServiceAPIKeysAsync(serviceName);
            return Ok(Result<Dictionary<string, string>>.SuccessResult(keys));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Result<Dictionary<string, string>>.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get a specific API key value
    /// </summary>
    [HttpGet("{serviceName}/{keyName}")]
    [ProducesResponseType(typeof(Result<string>), 200)]
    public async Task<IActionResult> GetAPIKey(string serviceName, string keyName)
    {
        try
        {
            var value = await _apiKeyService.GetAPIKeyAsync(serviceName, keyName);
            
            if (value == null)
            {
                return NotFound(Result<string>.FailureResult("NOT_FOUND", $"API key '{serviceName}:{keyName}' not found"));
            }

            return Ok(Result<string>.SuccessResult(value));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Result<string>.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Create or update an API key
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> CreateOrUpdateAPIKey([FromBody] CreateOrUpdateAPIKeyRequest request)
    {
        // Validate request
        if (request == null)
        {
            return BadRequest(Result<bool>.FailureResult("INVALID_REQUEST", "Request body is required"));
        }

        if (string.IsNullOrWhiteSpace(request.ServiceName))
        {
            return BadRequest(Result<bool>.FailureResult("INVALID_REQUEST", "ServiceName is required"));
        }

        if (string.IsNullOrWhiteSpace(request.KeyName))
        {
            return BadRequest(Result<bool>.FailureResult("INVALID_REQUEST", "KeyName is required"));
        }

        if (string.IsNullOrWhiteSpace(request.KeyValue))
        {
            return BadRequest(Result<bool>.FailureResult("INVALID_REQUEST", "KeyValue is required"));
        }

        var userId = _currentUserService.UserId;
        
        try
        {
            var success = await _apiKeyService.UpdateAPIKeyAsync(
                request.ServiceName,
                request.KeyName,
                request.KeyValue,
                userId);

            if (!success)
            {
                return BadRequest(Result<bool>.FailureResult("UPDATE_FAILED", "Failed to update API key. Please check server logs for details."));
            }

            return Ok(Result<bool>.SuccessResult(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating API key: {ServiceName}:{KeyName}", request.ServiceName, request.KeyName);
            return StatusCode(500, Result<bool>.FailureResult("SERVER_ERROR", $"An error occurred: {ex.Message}. Inner: {ex.InnerException?.Message}"));
        }
    }

    /// <summary>
    /// Update an existing API key
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> UpdateAPIKey(Guid id, [FromBody] UpdateAPIKeyRequest request)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(id);
        
        if (apiKey == null)
        {
            return NotFound(Result<bool>.FailureResult("NOT_FOUND", "API key not found"));
        }

        var userId = _currentUserService.UserId;
        
        var success = await _apiKeyService.UpdateAPIKeyAsync(
            apiKey.ServiceName,
            apiKey.KeyName,
            request.KeyValue,
            userId);

        if (!success)
        {
            return BadRequest(Result<bool>.FailureResult("UPDATE_FAILED", "Failed to update API key"));
        }

        return Ok(Result<bool>.SuccessResult(true));
    }

    /// <summary>
    /// Toggle API key active status
    /// </summary>
    [HttpPatch("{id}/toggle-active")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(id);
        
        if (apiKey == null)
        {
            return NotFound(Result<bool>.FailureResult("NOT_FOUND", "API key not found"));
        }

        apiKey.IsActive = !apiKey.IsActive;
        apiKey.UpdatedAt = DateTime.UtcNow;
        apiKey.UpdatedBy = _currentUserService.UserId;

        await _apiKeyRepository.UpdateAsync(apiKey);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        _apiKeyService.ClearCache(apiKey.ServiceName, apiKey.KeyName);

        return Ok(Result<bool>.SuccessResult(apiKey.IsActive));
    }

    /// <summary>
    /// Delete an API key
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> DeleteAPIKey(Guid id)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(id);
        
        if (apiKey == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "API key not found"));
        }

        await _apiKeyRepository.DeleteAsync(apiKey);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        _apiKeyService.ClearCache(apiKey.ServiceName, apiKey.KeyName);

        return Ok(Result.SuccessResult("API key deleted successfully"));
    }
}

public class APIKeyResponse
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateOrUpdateAPIKeyRequest
{
    public string ServiceName { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty;
    public string? Environment { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateAPIKeyRequest
{
    public string KeyValue { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

