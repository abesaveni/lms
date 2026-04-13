using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using LiveExpert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.Infrastructure.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly ApplicationDbContext _context;
    private static readonly Dictionary<string, object> _cache = new();
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public SystemSettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<SystemSetting?> GetSettingEntityAsync(string key)
    {
        // Check cache
        if (DateTime.UtcNow < _cacheExpiry && _cache.ContainsKey(key))
        {
            return _cache[key] as SystemSetting;
        }

        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);

        // Update cache
        if (setting != null)
        {
            _cache[key] = setting;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
        }

        return setting;
    }

    public async Task<T?> GetSettingAsync<T>(string key, T? defaultValue = default)
    {
        var setting = await GetSettingEntityAsync(key);
        if (setting == null)
        {
            return defaultValue;
        }

        try
        {
            return setting.DataType switch
            {
                "Bool" => (T)(object)bool.Parse(setting.SettingValue),
                "Int" => (T)(object)int.Parse(setting.SettingValue),
                "Decimal" => (T)(object)decimal.Parse(setting.SettingValue),
                _ => (T)(object)setting.SettingValue
            };
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task<decimal> GetMinWithdrawalAmountAsync()
    {
        return await GetSettingAsync<decimal>("MinWithdrawalAmount", 1000.0m);
    }

    public async Task<decimal> GetReferralBonusCreditsAsync()
    {
        return await GetSettingAsync<decimal>("ReferralBonusCredits", 500.0m);
    }

    public async Task<decimal> GetRegistrationBonusCreditsAsync()
    {
        return await GetSettingAsync<decimal>("RegistrationBonusCredits", 100.0m);
    }

    public async Task<bool> IsPlatformFeeEnabledAsync()
    {
        return await GetSettingAsync<bool>("PlatformFeeEnabled", true);
    }

    public async Task<PlatformFeeType> GetPlatformFeeTypeAsync()
    {
        var type = await GetSettingAsync<string>("PlatformFeeType", "Fixed");
        return Enum.TryParse(type, true, out PlatformFeeType parsed) ? parsed : PlatformFeeType.Fixed;
    }

    public async Task<decimal> GetPlatformFeeFixedAsync()
    {
        return await GetSettingAsync<decimal>("PlatformFeeFixed", 100.0m);
    }

    public async Task<decimal> GetPlatformFeePerHourAsync()
    {
        return await GetSettingAsync<decimal>("PlatformFeePerHour", 50.0m);
    }

    public async Task<decimal> GetPlatformFeePercentageAsync()
    {
        return await GetSettingAsync<decimal>("PlatformFeePercentage", 0.0m);
    }
}




