using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

public class APIKeyService : IAPIKeyService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<APIKeyService> _logger;
    
    // Cache for API keys (serviceName:keyName -> value)
    private static readonly Dictionary<string, string> _cache = new();
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly object _cacheLock = new();

    public APIKeyService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IEncryptionService encryptionService,
        ILogger<APIKeyService> logger)
    {
        _context = context;
        _configuration = configuration;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<string?> GetAPIKeyAsync(string serviceName, string keyName, string? defaultValue = null)
    {
        var cacheKey = $"{serviceName}:{keyName}";
        
        // Check cache first
        lock (_cacheLock)
        {
            if (DateTime.UtcNow < _cacheExpiry && _cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }
        }

        try
        {
            // Try to get from database first
            var apiKey = await _context.APIKeys
                .FirstOrDefaultAsync(k => 
                    k.ServiceName == serviceName && 
                    k.KeyName == keyName && 
                    k.IsActive &&
                    (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));

            if (apiKey != null && !string.IsNullOrEmpty(apiKey.KeyValue))
            {
                // Decrypt if encrypted
                string decryptedValue;
                try
                {
                    decryptedValue = _encryptionService.Decrypt(apiKey.KeyValue);
                }
                catch
                {
                    // If decryption fails, assume it's stored in plain text (for backward compatibility)
                    decryptedValue = apiKey.KeyValue;
                }

                // Update cache
                lock (_cacheLock)
                {
                    _cache[cacheKey] = decryptedValue;
                    _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                }

                return decryptedValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve API key from database for {ServiceName}:{KeyName}", serviceName, keyName);
        }

        // Fallback to configuration
        var configKey = $"{serviceName}:{keyName}";
        var configValue = _configuration[configKey];
        
        if (!string.IsNullOrEmpty(configValue))
        {
            // Update cache
            lock (_cacheLock)
            {
                _cache[cacheKey] = configValue;
                _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
            }
            
            return configValue;
        }

        // Return default value if provided
        if (defaultValue != null)
        {
            return defaultValue;
        }

        _logger.LogWarning("API key not found: {ServiceName}:{KeyName}", serviceName, keyName);
        return null;
    }

    public async Task<Dictionary<string, string>> GetServiceAPIKeysAsync(string serviceName)
    {
        var result = new Dictionary<string, string>();

        try
        {
            var apiKeys = await _context.APIKeys
                .Where(k => k.ServiceName == serviceName && k.IsActive)
                .ToListAsync();

            foreach (var apiKey in apiKeys)
            {
                try
                {
                    string decryptedValue;
                    try
                    {
                        decryptedValue = _encryptionService.Decrypt(apiKey.KeyValue);
                    }
                    catch
                    {
                        decryptedValue = apiKey.KeyValue; // Plain text fallback
                    }

                    result[apiKey.KeyName] = decryptedValue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt API key {ServiceName}:{KeyName}", serviceName, apiKey.KeyName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve API keys for service {ServiceName}", serviceName);
        }

        return result;
    }

    public async Task<bool> UpdateAPIKeyAsync(string serviceName, string keyName, string keyValue, Guid? updatedBy = null)
    {
        try
        {
            var apiKey = await _context.APIKeys
                .FirstOrDefaultAsync(k => k.ServiceName == serviceName && k.KeyName == keyName);

            // Encrypt the value before storing
            var encryptedValue = _encryptionService.Encrypt(keyValue);

            if (apiKey == null)
            {
                // Create new API key
                apiKey = new APIKey
                {
                    Id = Guid.NewGuid(),
                    ServiceName = serviceName,
                    KeyName = keyName,
                    KeyValue = encryptedValue,
                    IsActive = true,
                    Environment = "Production",
                    UpdatedBy = updatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.APIKeys.AddAsync(apiKey);
            }
            else
            {
                // Update existing
                apiKey.KeyValue = encryptedValue;
                apiKey.UpdatedBy = updatedBy;
                apiKey.UpdatedAt = DateTime.UtcNow;
                _context.APIKeys.Update(apiKey);
            }

            await _context.SaveChangesAsync();

            // Clear cache for this key
            ClearCache(serviceName, keyName);

            _logger.LogInformation("API key updated: {ServiceName}:{KeyName}", serviceName, keyName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update API key {ServiceName}:{KeyName}", serviceName, keyName);
            return false;
        }
    }

    public void ClearCache(string? serviceName = null, string? keyName = null)
    {
        lock (_cacheLock)
        {
            if (serviceName == null && keyName == null)
            {
                // Clear all cache
                _cache.Clear();
                _cacheExpiry = DateTime.MinValue;
            }
            else if (keyName != null)
            {
                // Clear specific key
                var cacheKey = $"{serviceName}:{keyName}";
                _cache.Remove(cacheKey);
            }
            else
            {
                // Clear all keys for a service
                var keysToRemove = _cache.Keys.Where(k => k.StartsWith($"{serviceName}:")).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }
        }
    }
}



