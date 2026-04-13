using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers.Admin;

/// <summary>
/// System Settings Management (Admin only)
/// </summary>
[Route("api/admin/settings")]
[ApiController]
[Authorize(Roles = "Admin")]
public class SystemSettingsController : ControllerBase
{
    private readonly IRepository<SystemSetting> _settingsRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SystemSettingsController(
        IRepository<SystemSetting> settingsRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _settingsRepository = settingsRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all system settings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<Dictionary<string, object>>), 200)]
    public async Task<IActionResult> GetAllSettings()
    {
        var settings = await _settingsRepository.GetAllAsync();
        var settingsDict = new Dictionary<string, object>();

        foreach (var setting in settings)
        {
            object value = setting.DataType switch
            {
                "Bool" => bool.Parse(setting.SettingValue),
                "Int" => int.Parse(setting.SettingValue),
                "Decimal" => decimal.Parse(setting.SettingValue),
                "JSON" => System.Text.Json.JsonSerializer.Deserialize<object>(setting.SettingValue) ?? setting.SettingValue,
                _ => setting.SettingValue
            };
            settingsDict[setting.SettingKey] = value;
        }

        return Ok(Result<Dictionary<string, object>>.SuccessResult(settingsDict));
    }

    /// <summary>
    /// Get a specific setting by key
    /// </summary>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(Result<object>), 200)]
    public async Task<IActionResult> GetSetting(string key)
    {
        var setting = await _settingsRepository.FirstOrDefaultAsync(s => s.SettingKey == key);

        if (setting == null)
        {
            return NotFound(Result<object>.FailureResult("NOT_FOUND", $"Setting '{key}' not found"));
        }

        object value = setting.DataType switch
        {
            "Bool" => bool.Parse(setting.SettingValue),
            "Int" => int.Parse(setting.SettingValue),
            "Decimal" => decimal.Parse(setting.SettingValue),
            "JSON" => System.Text.Json.JsonSerializer.Deserialize<object>(setting.SettingValue) ?? setting.SettingValue,
            _ => setting.SettingValue
        };

        return Ok(Result<object>.SuccessResult(value));
    }

    /// <summary>
    /// Update a system setting
    /// </summary>
    [HttpPut("{key}")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request)
    {
        var setting = await _settingsRepository.FirstOrDefaultAsync(s => s.SettingKey == key);

        if (setting == null)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", $"Setting '{key}' not found"));
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        // Convert value to string based on data type
        string stringValue = setting.DataType switch
        {
            "Bool" => request.Value.ToString()?.ToLower() ?? "false",
            "Int" => request.Value.ToString() ?? "0",
            "Decimal" => request.Value.ToString() ?? "0",
            "JSON" => System.Text.Json.JsonSerializer.Serialize(request.Value),
            _ => request.Value?.ToString() ?? string.Empty
        };

        setting.SettingValue = stringValue;
        setting.UpdatedBy = userId.Value;
        setting.UpdatedAt = DateTime.UtcNow;

        await _settingsRepository.UpdateAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.SuccessResult("Setting updated successfully"));
    }

    /// <summary>
    /// Get public settings (non-sensitive, for frontend)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<Dictionary<string, object>>), 200)]
    public async Task<IActionResult> GetPublicSettings()
    {
        // Only return settings that are safe to expose publicly
        var publicKeys = new[] { 
            "PlatformCommission",
            "MinWithdrawalAmount"
        };
        
        var settings = await _settingsRepository.GetAllAsync();
        var publicSettings = settings
            .Where(s => publicKeys.Contains(s.SettingKey))
            .ToDictionary(
                s => s.SettingKey,
                s => s.DataType switch
                {
                    "Bool" => (object)bool.Parse(s.SettingValue),
                    "Int" => int.Parse(s.SettingValue),
                    "Decimal" => decimal.Parse(s.SettingValue),
                    _ => s.SettingValue
                }
            );

        return Ok(Result<Dictionary<string, object>>.SuccessResult(publicSettings));
    }
}

public class UpdateSettingRequest
{
    public object Value { get; set; } = null!;
}

