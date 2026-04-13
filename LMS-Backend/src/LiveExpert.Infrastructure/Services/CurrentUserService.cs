using LiveExpert.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LiveExpert.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
