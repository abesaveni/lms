namespace LiveExpert.Application.Interfaces;

public interface IApiSettingService
{
    Task<string?> GetApiSettingAsync(string provider, string keyName);
    Task<bool> SetApiSettingAsync(string provider, string keyName, string value, string? description = null);
    Task<Dictionary<string, string>> GetAllApiSettingsAsync(string provider);
    Task<bool> DeleteApiSettingAsync(string provider, string keyName);
    Task<bool> UpdateEnvFileAsync(string envKey, string value);
}
