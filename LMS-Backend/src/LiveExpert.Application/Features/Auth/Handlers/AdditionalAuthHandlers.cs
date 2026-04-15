using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Auth.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Auth.Handlers;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<LoginResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<StudentProfile> _studentRepository;
    private readonly IRepository<BonusPoint> _bonusPointRepository;
    private readonly ISystemSettingsService _settingsService;
    private readonly IRepository<UserConsent> _consentRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public GoogleLoginCommandHandler(
        IRepository<User> userRepository,
        IRepository<StudentProfile> studentRepository,
        IRepository<BonusPoint> bonusPointRepository,
        ISystemSettingsService settingsService,
        IRepository<UserConsent> consentRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _bonusPointRepository = bonusPointRepository;
        _settingsService = settingsService;
        _consentRepository = consentRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // TODO: Verify Google ID token with Google API
        // For now, we'll extract email from token (stub implementation)
        
        // In production, decode and verify the ID token
        var email = "google_user@example.com"; // Placeholder
        var username = email.Split('@')[0] + new Random().Next(1000, 9999);

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            // Create new user
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    Email = email,
                    PasswordHash = "", // No password for OAuth users
                    Role = request.Role,
                    IsEmailVerified = true, // Google email is verified
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user, cancellationToken);

                if (request.Role == Domain.Enums.UserRole.Student)
                {
                    // Create student profile
                    var studentProfile = new StudentProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ReferralCode = GenerateReferralCode(user.Username),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _studentRepository.AddAsync(studentProfile, cancellationToken);

                    var registrationBonus = await _settingsService.GetRegistrationBonusCreditsAsync();

                    if (registrationBonus > 0)
                    {
                        var bonusPoint = new BonusPoint
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            Points = (int)Math.Round(registrationBonus),
                            Reason = BonusPointReason.Registration,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _bonusPointRepository.AddAsync(bonusPoint, cancellationToken);
                    }
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Track Google Login consent (IP and UserAgent will be tracked in the controller)
        try
        {
            var existingConsent = await _consentRepository.FirstOrDefaultAsync(
                c => c.UserId == user.Id && c.ConsentType == ConsentType.GoogleLogin, cancellationToken);
            
            if (existingConsent != null)
            {
                existingConsent.Granted = true;
                existingConsent.GrantedAt = DateTime.UtcNow;
                existingConsent.RevokedAt = null;
                await _consentRepository.UpdateAsync(existingConsent, cancellationToken);
            }
            else
            {
                var newConsent = new UserConsent
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ConsentType = ConsentType.GoogleLogin,
                    Granted = true,
                    GrantedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _consentRepository.AddAsync(newConsent, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail login
            System.Diagnostics.Debug.WriteLine($"Failed to track Google login consent: {ex.Message}");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return Result<LoginResponse>.SuccessResult(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                ProfileImage = user.ProfileImageUrl
            }
        });
    }

    private string GenerateReferralCode(string username)
    {
        var prefix = username.ToUpper().Substring(0, Math.Min(3, username.Length));
        var unique = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"{prefix}{unique}";
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserService _currentUserService;

    public RefreshTokenCommandHandler(
        IRepository<User> userRepository,
        IJwtTokenService jwtTokenService,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // TODO: Validate refresh token from database
        // For now, we'll use the current user from context

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<LoginResponse>.FailureResult("INVALID_TOKEN", "Invalid refresh token");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return Result<LoginResponse>.FailureResult("USER_NOT_FOUND", "User not found or inactive");
        }

        // Generate new tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return Result<LoginResponse>.SuccessResult(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                ProfileImage = user.ProfileImageUrl
            }
        });
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        IRepository<User> userRepository,
        IEncryptionService encryptionService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        // Validate new password complexity (validator not in MediatR pipeline)
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Result.FailureResult("WEAK_PASSWORD", "New password must be at least 8 characters");
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"(?=.*[a-z])(?=.*[A-Z])(?=.*\d)"))
        {
            return Result.FailureResult("WEAK_PASSWORD", "New password must contain at least one uppercase letter, one lowercase letter, and one number");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result.FailureResult("USER_NOT_FOUND", "User not found");
        }

        // Prevent super admin password change
        if (user.Email == "superadmin@liveexpert.ai")
        {
            return Result.FailureResult("FORBIDDEN", "Super admin password cannot be changed");
        }

        // Verify current password
        if (!_encryptionService.VerifyHash(request.CurrentPassword, user.PasswordHash))
        {
            return Result.FailureResult("INVALID_PASSWORD", "Current password is incorrect");
        }

        // Update password
        user.PasswordHash = _encryptionService.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Password changed successfully");
    }
}
