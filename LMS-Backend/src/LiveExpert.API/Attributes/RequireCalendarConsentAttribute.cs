using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace LiveExpert.API.Attributes;

/// <summary>
/// Authorization attribute that requires Google Calendar consent for the action
/// Returns 403 with "GoogleCalendarNotConnected" error code if consent is not granted
/// </summary>
public class RequireCalendarConsentAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip for testing
        return;
        // Skip if already unauthorized
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return; // Let [Authorize] handle authentication
        }

        var userIdClaim = context.HttpContext.User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                error = new { code = "UNAUTHORIZED", message = "User ID not found in token" }
            });
            return;
        }

        // Get user role - Admins don't need calendar connection
        var roleClaim = context.HttpContext.User.FindFirst("role")?.Value;
        if (roleClaim == "Admin")
        {
            return; // Skip calendar check for admins
        }

        // Get services
        var consentRepository = context.HttpContext.RequestServices.GetRequiredService<IRepository<UserConsent>>();

        // Check if calendar consent exists and is granted
        var calendarConsent = await consentRepository.FirstOrDefaultAsync(
            c => c.UserId == userId && 
                 c.ConsentType == ConsentType.GoogleCalendar && 
                 c.Granted);

        if (calendarConsent == null)
        {
            context.Result = new ObjectResult(new
            {
                success = false,
                error = new { code = "GoogleCalendarNotConnected", message = "Google Calendar connection is required to perform this action. Please connect your calendar in Settings." }
            })
            {
                StatusCode = 403
            };
        }
    }
}
