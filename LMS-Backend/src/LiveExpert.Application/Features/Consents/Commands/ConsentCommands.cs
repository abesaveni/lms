using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Consents.Commands;

// Cookie Consent Commands
public class SaveCookieConsentCommand : IRequest<Result<CookieConsentDto>>
{
    public bool Necessary { get; set; } = true;
    public bool Functional { get; set; }
    public bool Analytics { get; set; }
    public bool Marketing { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class UpdateCookieConsentCommand : IRequest<Result<CookieConsentDto>>
{
    public bool Functional { get; set; }
    public bool Analytics { get; set; }
    public bool Marketing { get; set; }
}

public class GetCookieConsentQuery : IRequest<Result<CookieConsentDto?>>
{
    // Uses current user or anonymous
}

public class CookieConsentDto
{
    public Guid? Id { get; set; }
    public bool Necessary { get; set; } = true;
    public bool Functional { get; set; }
    public bool Analytics { get; set; }
    public bool Marketing { get; set; }
    public DateTime ConsentGivenAt { get; set; }
    public DateTime? ConsentUpdatedAt { get; set; }
}

// User Consent Commands (Google OAuth)
public class SaveUserConsentCommand : IRequest<Result<UserConsentDto>>
{
    public ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class RevokeUserConsentCommand : IRequest<Result>
{
    public ConsentType ConsentType { get; set; }
}

public class GetUserConsentsQuery : IRequest<Result<List<UserConsentDto>>>
{
    // Uses current user
}

public class UserConsentDto
{
    public Guid Id { get; set; }
    public ConsentType ConsentType { get; set; }
    public string ConsentTypeName { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public DateTime? GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

// Admin Queries
public class GetCookieConsentsQuery : IRequest<Result<PaginatedResult<CookieConsentAdminDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? UserId { get; set; }
}

public class GetUserConsentsAdminQuery : IRequest<Result<PaginatedResult<UserConsentAdminDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? UserId { get; set; }
    public ConsentType? ConsentType { get; set; }
}

public class CookieConsentAdminDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public bool Necessary { get; set; }
    public bool Functional { get; set; }
    public bool Analytics { get; set; }
    public bool Marketing { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ConsentGivenAt { get; set; }
    public DateTime? ConsentUpdatedAt { get; set; }
}

public class UserConsentAdminDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public ConsentType ConsentType { get; set; }
    public string ConsentTypeName { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public DateTime? GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
