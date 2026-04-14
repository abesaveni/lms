using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Auth.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Enums;
using LiveExpert.Domain.Entities;
using MediatR;

namespace LiveExpert.Application.Features.Auth.Handlers;

// Email Verification Handler
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public VerifyEmailCommandHandler(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.FailureResult("USER_NOT_FOUND", "User not found");
        }

        if (user.IsEmailVerified)
        {
            return Result.FailureResult("ALREADY_VERIFIED", "Email is already verified");
        }

        if (string.IsNullOrWhiteSpace(user.EmailVerificationToken) ||
            user.EmailVerificationTokenExpiresAt == null ||
            user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            return Result.FailureResult("TOKEN_EXPIRED", "Verification token has expired");
        }

        if (!string.Equals(user.EmailVerificationToken, request.Token, StringComparison.Ordinal))
        {
            return Result.FailureResult("INVALID_TOKEN", "Invalid verification token");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send welcome notification
        await _notificationService.SendWelcomeMessageAsync(user, cancellationToken);

        return Result.SuccessResult("Email verified successfully");
    }
}

// Resend Email Verification Handler
public class ResendEmailVerificationCommandHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ResendEmailVerificationCommandHandler(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.FailureResult("USER_NOT_FOUND", "User not found");
        }

        if (user.IsEmailVerified)
        {
            return Result.FailureResult("ALREADY_VERIFIED", "Email is already verified");
        }

        var token = new Random().Next(100000, 999999).ToString();
        user.EmailVerificationToken = token;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (user.Role == UserRole.Tutor)
        {
            var (subject, body) = NotificationTemplates.EmailVerification(user.FirstName, token, 30);
            await _emailService.SendEmailAsync(user.Email, subject, body, false);
        }
        else
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Verify your email",
                EmailTemplates.VerificationEmail(user.FirstName, token, 30),
                true
            );
        }

        return Result.SuccessResult("Verification email resent successfully");
    }
}

// WhatsApp Verification Handler
public class VerifyWhatsAppCommandHandler : IRequestHandler<VerifyWhatsAppCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public VerifyWhatsAppCommandHandler(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(VerifyWhatsAppCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.FailureResult("USER_NOT_FOUND", "User not found");
        }

        if (user.IsWhatsAppVerified)
        {
            return Result.FailureResult("ALREADY_VERIFIED", "WhatsApp is already verified");
        }

        // TODO: Validate OTP (in production, check against stored OTP)
        // For now, we'll accept any OTP for development

        user.IsWhatsAppVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send confirmation notification
        await _notificationService.SendNotificationAsync(
            user.Id,
            "WhatsApp Verified!",
            "Your WhatsApp number has been successfully verified.",
            null,
            null
        );

        return Result.SuccessResult("WhatsApp verified successfully");
    }
}

// Forgot Password Handler
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        IRepository<User> userRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        // Don't reveal if user exists or not (security best practice)
        if (user == null)
        {
            return Result.SuccessResult("If the email exists, a password reset link has been sent");
        }

        // Generate reset token and expiration
        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resetLink = $"https://liveexpert.ai/reset-password?token={resetToken}&userId={user.Id}";

        // Send password reset email
        await _notificationService.SendForgotPasswordEmailAsync(user, resetLink, 60, cancellationToken);

        return Result.SuccessResult("If the email exists, a password reset link has been sent");
    }
}

// Reset Password Handler
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IRepository<User> userRepository,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync((Guid)request.UserId, cancellationToken);
        
        if (user == null || 
            string.IsNullOrEmpty(user.PasswordResetToken) || 
            user.PasswordResetToken != request.Token ||
            user.PasswordResetTokenExpiresAt == null ||
            user.PasswordResetTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return Result.FailureResult("INVALID_TOKEN", "Invalid or expired reset token");
        }

        // Update password
        user.PasswordHash = _encryptionService.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Password reset successfully");
    }
}

// Logout Handler
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result.FailureResult("USER_NOT_FOUND", "User not found");
        }

        // TODO: In production, invalidate refresh token in database
        // For now, client should just discard the token

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Logged out successfully");
    }
}


