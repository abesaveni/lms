using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveExpert.API.Controllers;

[Authorize]
[Route("api/notification-preferences")]
[ApiController]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationPreferenceService _preferenceService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationPreferencesController(
        INotificationPreferenceService preferenceService,
        ICurrentUserService currentUserService)
    {
        _preferenceService = preferenceService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var preferences = await _preferenceService.GetOrCreateDefaultsAsync(userId.Value, cancellationToken);
        var response = preferences.Select(p => new NotificationPreferenceDto
        {
            Category = p.Category.ToString(),
            EmailEnabled = p.EmailEnabled,
            WhatsAppEnabled = p.WhatsAppEnabled,
            InAppEnabled = p.InAppEnabled
        }).ToList();

        return Ok(Result<List<NotificationPreferenceDto>>.SuccessResult(response));
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePreferences([FromBody] List<NotificationPreferenceDto> request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var preferences = new List<UserNotificationPreference>();
        foreach (var item in request)
        {
            if (!Enum.TryParse<NotificationCategory>(item.Category, out var category))
            {
                continue;
            }

            preferences.Add(new UserNotificationPreference
            {
                UserId = userId.Value,
                Category = category,
                EmailEnabled = item.EmailEnabled,
                WhatsAppEnabled = item.WhatsAppEnabled,
                InAppEnabled = item.InAppEnabled
            });
        }

        await _preferenceService.UpdatePreferencesAsync(userId.Value, preferences, cancellationToken);
        return Ok(Result.SuccessResult("Preferences updated"));
    }
}

public class NotificationPreferenceDto
{
    public string Category { get; set; } = string.Empty;
    public bool EmailEnabled { get; set; }
    public bool WhatsAppEnabled { get; set; }
    public bool InAppEnabled { get; set; }
}
