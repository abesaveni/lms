using LiveExpert.Domain.Enums;

namespace LiveExpert.Application.Interfaces;

public interface ISystemSettingsService
{
    Task<T?> GetSettingAsync<T>(string key, T? defaultValue = default);
    Task<decimal> GetReferralBonusCreditsAsync();
    Task<decimal> GetRegistrationBonusCreditsAsync();
    Task<bool> IsPlatformFeeEnabledAsync();
    Task<PlatformFeeType> GetPlatformFeeTypeAsync();
    Task<decimal> GetPlatformFeeFixedAsync();
    Task<decimal> GetPlatformFeePerHourAsync();
    Task<decimal> GetPlatformFeePercentageAsync();
    Task<decimal> GetMinWithdrawalAmountAsync();
    Task<decimal> GetMaxPayoutPercentageAsync();
}




