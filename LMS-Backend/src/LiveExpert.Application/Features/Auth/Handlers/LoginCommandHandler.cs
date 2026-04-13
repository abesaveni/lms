using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Auth.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IRepository<User> userRepository,
        IEncryptionService encryptionService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            // Log for debugging
            System.Diagnostics.Debug.WriteLine($"Login failed: User not found for email: {request.Email}");
            return Result<LoginResponse>.FailureResult("INVALID_CREDENTIALS", "Invalid email or password");
        }

        // Verify password
        var passwordValid = _encryptionService.VerifyHash(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            // Log for debugging
            System.Diagnostics.Debug.WriteLine($"Login failed: Invalid password for email: {request.Email}");
            return Result<LoginResponse>.FailureResult("INVALID_CREDENTIALS", "Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<LoginResponse>.FailureResult("FORBIDDEN", "Your account has been deactivated");
        }

        if (!user.IsEmailVerified)
        {
            return Result<LoginResponse>.FailureResult("EMAIL_NOT_VERIFIED", "Please verify your email before logging in");
        }

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
