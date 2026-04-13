using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Consents.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LiveExpert.Application.Features.Consents.Handlers;

// Cookie Consent Handlers
public class SaveCookieConsentCommandHandler : IRequestHandler<SaveCookieConsentCommand, Result<CookieConsentDto>>
{
    private readonly IRepository<CookieConsent> _consentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveCookieConsentCommandHandler(
        IRepository<CookieConsent> consentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _consentRepository = consentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CookieConsentDto>> Handle(SaveCookieConsentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var ipAddress = request.IpAddress;
        var userAgent = request.UserAgent;

        // Necessary cookies are always true
        request.Necessary = true;

        // Check if consent already exists
        CookieConsent? existingConsent = null;
        if (userId.HasValue)
        {
            existingConsent = await _consentRepository.FirstOrDefaultAsync(
                c => c.UserId == userId.Value, cancellationToken);
        }

        if (existingConsent != null)
        {
            // Update existing consent
            existingConsent.Functional = request.Functional;
            existingConsent.Analytics = request.Analytics;
            existingConsent.Marketing = request.Marketing;
            existingConsent.ConsentUpdatedAt = DateTime.UtcNow;
            existingConsent.IpAddress = ipAddress;
            existingConsent.UserAgent = userAgent;

            await _consentRepository.UpdateAsync(existingConsent, cancellationToken);
        }
        else
        {
            // Create new consent
            existingConsent = new CookieConsent
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Necessary = request.Necessary,
                Functional = request.Functional,
                Analytics = request.Analytics,
                Marketing = request.Marketing,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ConsentGivenAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _consentRepository.AddAsync(existingConsent, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CookieConsentDto>.SuccessResult(new CookieConsentDto
        {
            Id = existingConsent.Id,
            Necessary = existingConsent.Necessary,
            Functional = existingConsent.Functional,
            Analytics = existingConsent.Analytics,
            Marketing = existingConsent.Marketing,
            ConsentGivenAt = existingConsent.ConsentGivenAt,
            ConsentUpdatedAt = existingConsent.ConsentUpdatedAt
        });
    }
}

public class GetCookieConsentQueryHandler : IRequestHandler<GetCookieConsentQuery, Result<CookieConsentDto?>>
{
    private readonly IRepository<CookieConsent> _consentRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCookieConsentQueryHandler(
        IRepository<CookieConsent> consentRepository,
        ICurrentUserService currentUserService)
    {
        _consentRepository = consentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CookieConsentDto?>> Handle(GetCookieConsentQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
        {
            // Anonymous user - return null (consent stored in localStorage only)
            return Result<CookieConsentDto?>.SuccessResult(null);
        }

        var consent = await _consentRepository.FirstOrDefaultAsync(
            c => c.UserId == userId.Value, cancellationToken);

        if (consent == null)
        {
            return Result<CookieConsentDto?>.SuccessResult(null);
        }

        return Result<CookieConsentDto?>.SuccessResult(new CookieConsentDto
        {
            Id = consent.Id,
            Necessary = consent.Necessary,
            Functional = consent.Functional,
            Analytics = consent.Analytics,
            Marketing = consent.Marketing,
            ConsentGivenAt = consent.ConsentGivenAt,
            ConsentUpdatedAt = consent.ConsentUpdatedAt
        });
    }
}

// User Consent Handlers (Google OAuth)
public class SaveUserConsentCommandHandler : IRequestHandler<SaveUserConsentCommand, Result<UserConsentDto>>
{
    private readonly IRepository<UserConsent> _consentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveUserConsentCommandHandler(
        IRepository<UserConsent> consentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _consentRepository = consentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserConsentDto>> Handle(SaveUserConsentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<UserConsentDto>.FailureResult("UNAUTHORIZED", "User must be authenticated");
        }

        var ipAddress = request.IpAddress;
        var userAgent = request.UserAgent;

        // Check if consent already exists
        var existingConsent = await _consentRepository.FirstOrDefaultAsync(
            c => c.UserId == userId.Value && c.ConsentType == request.ConsentType, cancellationToken);

        if (existingConsent != null)
        {
            // Update existing consent
            existingConsent.Granted = request.Granted;
            if (request.Granted)
            {
                existingConsent.GrantedAt = DateTime.UtcNow;
                existingConsent.RevokedAt = null;
            }
            else
            {
                existingConsent.RevokedAt = DateTime.UtcNow;
            }
            existingConsent.IpAddress = ipAddress;
            existingConsent.UserAgent = userAgent;

            await _consentRepository.UpdateAsync(existingConsent, cancellationToken);
        }
        else
        {
            // Create new consent
            existingConsent = new UserConsent
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                ConsentType = request.ConsentType,
                Granted = request.Granted,
                GrantedAt = request.Granted ? DateTime.UtcNow : null,
                RevokedAt = request.Granted ? null : DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _consentRepository.AddAsync(existingConsent, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserConsentDto>.SuccessResult(new UserConsentDto
        {
            Id = existingConsent.Id,
            ConsentType = existingConsent.ConsentType,
            ConsentTypeName = existingConsent.ConsentType.ToString(),
            Granted = existingConsent.Granted,
            GrantedAt = existingConsent.GrantedAt,
            RevokedAt = existingConsent.RevokedAt
        });
    }
}

public class RevokeUserConsentCommandHandler : IRequestHandler<RevokeUserConsentCommand, Result>
{
    private readonly IRepository<UserConsent> _consentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeUserConsentCommandHandler(
        IRepository<UserConsent> consentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _consentRepository = consentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeUserConsentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User must be authenticated");
        }

        var consent = await _consentRepository.FirstOrDefaultAsync(
            c => c.UserId == userId.Value && c.ConsentType == request.ConsentType, cancellationToken);

        if (consent == null)
        {
            return Result.FailureResult("NOT_FOUND", "Consent not found");
        }

        consent.Granted = false;
        consent.RevokedAt = DateTime.UtcNow;

        await _consentRepository.UpdateAsync(consent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult();
    }
}

public class GetUserConsentsQueryHandler : IRequestHandler<GetUserConsentsQuery, Result<List<UserConsentDto>>>
{
    private readonly IRepository<UserConsent> _consentRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserConsentsQueryHandler(
        IRepository<UserConsent> consentRepository,
        ICurrentUserService currentUserService)
    {
        _consentRepository = consentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<UserConsentDto>>> Handle(GetUserConsentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<List<UserConsentDto>>.FailureResult("UNAUTHORIZED", "User must be authenticated");
        }

        var consents = await _consentRepository.FindAsync(
            c => c.UserId == userId.Value, cancellationToken);

        var dtos = consents.Select(c => new UserConsentDto
        {
            Id = c.Id,
            ConsentType = c.ConsentType,
            ConsentTypeName = c.ConsentType.ToString(),
            Granted = c.Granted,
            GrantedAt = c.GrantedAt,
            RevokedAt = c.RevokedAt
        }).ToList();

        return Result<List<UserConsentDto>>.SuccessResult(dtos);
    }
}

// Admin Handlers
public class GetCookieConsentsQueryHandler : IRequestHandler<GetCookieConsentsQuery, Result<PaginatedResult<CookieConsentAdminDto>>>
{
    private readonly IRepository<CookieConsent> _consentRepository;
    private readonly IRepository<User> _userRepository;

    public GetCookieConsentsQueryHandler(
        IRepository<CookieConsent> consentRepository,
        IRepository<User> userRepository)
    {
        _consentRepository = consentRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<PaginatedResult<CookieConsentAdminDto>>> Handle(GetCookieConsentsQuery request, CancellationToken cancellationToken)
    {
        var query = _consentRepository.GetQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(c => c.UserId == request.UserId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var consents = await query
            .OrderByDescending(c => c.ConsentGivenAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = consents.Where(c => c.UserId.HasValue).Select(c => c.UserId!.Value).Distinct().ToList();
        var users = userIds.Any() 
            ? await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken)
            : new List<User>();

        var dtos = consents.Select(c =>
        {
            var user = c.UserId.HasValue ? users.FirstOrDefault(u => u.Id == c.UserId.Value) : null;
            return new CookieConsentAdminDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserEmail = user?.Email,
                Necessary = c.Necessary,
                Functional = c.Functional,
                Analytics = c.Analytics,
                Marketing = c.Marketing,
                IpAddress = c.IpAddress,
                UserAgent = c.UserAgent,
                ConsentGivenAt = c.ConsentGivenAt,
                ConsentUpdatedAt = c.ConsentUpdatedAt
            };
        }).ToList();

        return Result<PaginatedResult<CookieConsentAdminDto>>.SuccessResult(
            new PaginatedResult<CookieConsentAdminDto>
            {
                Items = dtos,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                }
            });
    }
}

public class GetUserConsentsAdminQueryHandler : IRequestHandler<GetUserConsentsAdminQuery, Result<PaginatedResult<UserConsentAdminDto>>>
{
    private readonly IRepository<UserConsent> _consentRepository;
    private readonly IRepository<User> _userRepository;

    public GetUserConsentsAdminQueryHandler(
        IRepository<UserConsent> consentRepository,
        IRepository<User> userRepository)
    {
        _consentRepository = consentRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<PaginatedResult<UserConsentAdminDto>>> Handle(GetUserConsentsAdminQuery request, CancellationToken cancellationToken)
    {
        var query = _consentRepository.GetQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(c => c.UserId == request.UserId.Value);
        }

        if (request.ConsentType.HasValue)
        {
            query = query.Where(c => c.ConsentType == request.ConsentType.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var consents = await query
            .OrderByDescending(c => c.GrantedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = consents.Select(c => c.UserId).Distinct().ToList();
        var users = userIds.Any()
            ? await _userRepository.FindAsync(u => userIds.Contains(u.Id), cancellationToken)
            : new List<User>();

        var dtos = consents.Select(c =>
        {
            var user = users.FirstOrDefault(u => u.Id == c.UserId);
            return new UserConsentAdminDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserEmail = user?.Email ?? "Unknown",
                ConsentType = c.ConsentType,
                ConsentTypeName = c.ConsentType.ToString(),
                Granted = c.Granted,
                GrantedAt = c.GrantedAt,
                RevokedAt = c.RevokedAt,
                IpAddress = c.IpAddress,
                UserAgent = c.UserAgent
            };
        }).ToList();

        return Result<PaginatedResult<UserConsentAdminDto>>.SuccessResult(
            new PaginatedResult<UserConsentAdminDto>
            {
                Items = dtos,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                }
            });
    }
}
