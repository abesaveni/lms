using LiveExpert.Domain.Entities;
using System.Security.Claims;

namespace LiveExpert.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
