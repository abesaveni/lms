using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Auth.Commands;

// Register Command
public class RegisterCommand : IRequest<Result<RegisterResponse>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public UserRole Role { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ReferralCode { get; set; } // Optional referral code
    public string? SignupVerificationToken { get; set; }
}

public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EmailVerificationSent { get; set; }
    public bool WhatsAppVerificationSent { get; set; }
}

// Login Command
public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; } = 3600;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
}

// Google Login Command
public class GoogleLoginCommand : IRequest<Result<LoginResponse>>
{
    public string IdToken { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

// Verify Email Command
public class VerifyEmailCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class ResendEmailVerificationCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}

// Verify WhatsApp Command
public class VerifyWhatsAppCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string Otp { get; set; } = string.Empty;
}

// Refresh Token Command
public class RefreshTokenCommand : IRequest<Result<LoginResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

// Forgot Password Command
public class ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
}

// Reset Password Command
public class ResetPasswordCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

// Change Password Command
public class ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

// Logout Command
public class LogoutCommand : IRequest<Result>
{
    // No properties needed - uses current user from context
}
