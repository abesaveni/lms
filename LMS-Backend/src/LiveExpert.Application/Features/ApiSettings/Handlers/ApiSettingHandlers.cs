using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using MediatR;
using LiveExpert.Application.Features.ApiSettings.Commands;

namespace LiveExpert.Application.Features.ApiSettings.Handlers;

public class UpdateApiSettingCommandHandler : IRequestHandler<UpdateApiSettingCommand, Result<bool>>
{
    private readonly IApiSettingService _apiSettingService;

    public UpdateApiSettingCommandHandler(IApiSettingService apiSettingService)
    {
        _apiSettingService = apiSettingService;
    }

    public async Task<Result<bool>> Handle(UpdateApiSettingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Update in database
            var success = await _apiSettingService.SetApiSettingAsync(
                request.Provider,
                request.KeyName,
                request.Value,
                request.Description);

            if (!success)
            {
                return Result<bool>.FailureResult("UPDATE_FAILED", "Failed to update API setting in database");
            }

            // Update .env file if requested
            if (request.UpdateEnvFile)
            {
                // Map provider and key name to .env variable name
                var envKey = MapToEnvKey(request.Provider, request.KeyName);
                if (!string.IsNullOrEmpty(envKey))
                {
                    await _apiSettingService.UpdateEnvFileAsync(envKey, request.Value);
                }
            }

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.FailureResult("ERROR", $"Error updating API setting: {ex.Message}");
        }
    }

    private string? MapToEnvKey(string provider, string keyName)
    {
        // Map API setting keys to .env variable names
        return (provider, keyName) switch
        {
            ("GoogleOAuth", "ClientId") => "GOOGLE_CLIENT_ID",
            ("GoogleOAuth", "ClientSecret") => "GOOGLE_CLIENT_SECRET",
            ("GoogleOAuth", "RedirectUri") => "GOOGLE_REDIRECT_URI",
            ("GoogleCalendar", "ClientId") => "GOOGLE_CALENDAR_CLIENT_ID",
            ("GoogleCalendar", "ClientSecret") => "GOOGLE_CALENDAR_CLIENT_SECRET",
            ("GoogleCalendar", "RedirectUri") => "GOOGLE_CALENDAR_REDIRECT_URI",
            ("Email", "SmtpHost") => "MAIL_HOST",
            ("Email", "SmtpPort") => "MAIL_PORT",
            ("Email", "Username") => "MAIL_USERNAME",
            ("Email", "Password") => "MAIL_PASSWORD",
            ("Email", "Encryption") => "MAIL_ENCRYPTION",
            ("Email", "FromAddress") => "MAIL_FROM_ADDRESS",
            ("Email", "FromName") => "MAIL_FROM_NAME",
            ("Email", "Mailer") => "MAIL_MAILER",
            _ => null
        };
    }
}

public class GetApiSettingsQueryHandler : IRequestHandler<GetApiSettingsCommand, Result<Dictionary<string, string>>>
{
    private readonly IApiSettingService _apiSettingService;

    public GetApiSettingsQueryHandler(IApiSettingService apiSettingService)
    {
        _apiSettingService = apiSettingService;
    }

    public async Task<Result<Dictionary<string, string>>> Handle(GetApiSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _apiSettingService.GetAllApiSettingsAsync(request.Provider);
            return Result<Dictionary<string, string>>.SuccessResult(settings);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, string>>.FailureResult("ERROR", $"Error retrieving API settings: {ex.Message}");
        }
    }
}

public class DeleteApiSettingCommandHandler : IRequestHandler<DeleteApiSettingCommand, Result<bool>>
{
    private readonly IApiSettingService _apiSettingService;

    public DeleteApiSettingCommandHandler(IApiSettingService apiSettingService)
    {
        _apiSettingService = apiSettingService;
    }

    public async Task<Result<bool>> Handle(DeleteApiSettingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _apiSettingService.DeleteApiSettingAsync(request.Provider, request.KeyName);
            return success
                ? Result<bool>.SuccessResult(true)
                : Result<bool>.FailureResult("NOT_FOUND", "API setting not found or already deleted");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailureResult("ERROR", $"Error deleting API setting: {ex.Message}");
        }
    }
}
