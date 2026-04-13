using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

public class ApiSettingService : IApiSettingService
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly EnvFileService _envFileService;
    private readonly ILogger<ApiSettingService> _logger;
    private static readonly Dictionary<string, string> _cache = new();
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly object _cacheLock = new();

    public ApiSettingService(
        ApplicationDbContext context,
        IEncryptionService encryptionService,
        EnvFileService envFileService,
        ILogger<ApiSettingService> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _envFileService = envFileService;
        _logger = logger;
    }

    public async Task<string?> GetApiSettingAsync(string provider, string keyName)
    {
        var cacheKey = $"{provider}:{keyName}";

        // Check cache
        lock (_cacheLock)
        {
            if (DateTime.UtcNow < _cacheExpiry && _cache.TryGetValue(cacheKey, out var cachedValue))
            {
                return cachedValue;
            }
        }

        // Parse provider enum
        if (!Enum.TryParse<ApiProvider>(provider, out var apiProvider))
        {
            _logger.LogWarning("Invalid API provider: {Provider}", provider);
            return null;
        }

        // Get from database
        var setting = await _context.ApiSettings
            .FirstOrDefaultAsync(s => s.Provider == apiProvider && 
                                     s.KeyName == keyName && 
                                     s.IsActive);

        if (setting == null)
        {
            _logger.LogWarning("API setting not found: {Provider}:{KeyName}", provider, keyName);
            return null;
        }

        // Decrypt value
        var decryptedValue = _encryptionService.Decrypt(setting.KeyValue);

        // Update cache
        lock (_cacheLock)
        {
            _cache[cacheKey] = decryptedValue;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
        }

        return decryptedValue;
    }

    public async Task<bool> SetApiSettingAsync(string provider, string keyName, string value, string? description = null)
    {
        try
        {
            // Parse provider enum
            if (!Enum.TryParse<ApiProvider>(provider, out var apiProvider))
            {
                _logger.LogWarning("Invalid API provider: {Provider}", provider);
                return false;
            }

            // Encrypt value
            var encryptedValue = _encryptionService.Encrypt(value);

            // Find existing setting
            var existingSetting = await _context.ApiSettings
                .FirstOrDefaultAsync(s => s.Provider == apiProvider && s.KeyName == keyName);

            if (existingSetting != null)
            {
                // Update existing
                existingSetting.KeyValue = encryptedValue;
                existingSetting.IsActive = true;
                existingSetting.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(description))
                {
                    existingSetting.Description = description;
                }
            }
            else
            {
                // Create new
                var newSetting = new ApiSetting
                {
                    Id = Guid.NewGuid(),
                    Provider = apiProvider,
                    KeyName = keyName,
                    KeyValue = encryptedValue,
                    IsActive = true,
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.ApiSettings.AddAsync(newSetting);
            }

            await _context.SaveChangesAsync();

            // Clear cache
            var cacheKey = $"{provider}:{keyName}";
            lock (_cacheLock)
            {
                _cache.Remove(cacheKey);
            }

            _logger.LogInformation("API setting updated: {Provider}:{KeyName}", provider, keyName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set API setting: {Provider}:{KeyName}", provider, keyName);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetAllApiSettingsAsync(string provider)
    {
        var result = new Dictionary<string, string>();

        if (!Enum.TryParse<ApiProvider>(provider, out var apiProvider))
        {
            return result;
        }

        var settings = await _context.ApiSettings
            .Where(s => s.Provider == apiProvider && s.IsActive)
            .ToListAsync();

        foreach (var setting in settings)
        {
            try
            {
                var decryptedValue = _encryptionService.Decrypt(setting.KeyValue);
                result[setting.KeyName] = decryptedValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt setting: {Provider}:{KeyName}", provider, setting.KeyName);
            }
        }

        return result;
    }

    public async Task<bool> DeleteApiSettingAsync(string provider, string keyName)
    {
        try
        {
            if (!Enum.TryParse<ApiProvider>(provider, out var apiProvider))
            {
                return false;
            }

            var setting = await _context.ApiSettings
                .FirstOrDefaultAsync(s => s.Provider == apiProvider && s.KeyName == keyName);

            if (setting != null)
            {
                setting.IsActive = false;
                setting.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Clear cache
                var cacheKey = $"{provider}:{keyName}";
                lock (_cacheLock)
                {
                    _cache.Remove(cacheKey);
                }

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete API setting: {Provider}:{KeyName}", provider, keyName);
            return false;
        }
    }

    public async Task<bool> UpdateEnvFileAsync(string envKey, string value)
    {
        try
        {
            return await _envFileService.UpdateEnvVariableAsync(envKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update .env file for key: {Key}", envKey);
            return false;
        }
    }
}
