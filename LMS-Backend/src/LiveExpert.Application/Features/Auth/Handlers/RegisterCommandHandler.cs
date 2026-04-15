using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Auth.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace LiveExpert.Application.Features.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorProfile> _tutorRepository;
    private readonly IRepository<StudentProfile> _studentRepository;
    private readonly IRepository<BonusPoint> _bonusPointRepository;
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly IRepository<StudentProfile> _studentProfileRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly INotificationService _notificationService;
    private readonly ISystemSettingsService _settingsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;

    public RegisterCommandHandler(
        IRepository<User> userRepository,
        IRepository<TutorProfile> tutorRepository,
        IRepository<StudentProfile> studentRepository,
        IRepository<BonusPoint> bonusPointRepository,
        IRepository<ReferralProgram> referralRepository,
        IEncryptionService encryptionService,
        INotificationDispatcher notificationDispatcher,
        INotificationService notificationService,
        ISystemSettingsService settingsService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tutorRepository = tutorRepository;
        _studentRepository = studentRepository;
        _bonusPointRepository = bonusPointRepository;
        _referralRepository = referralRepository;
        _studentProfileRepository = studentRepository;
        _encryptionService = encryptionService;
        _notificationDispatcher = notificationDispatcher;
        _notificationService = notificationService;
        _settingsService = settingsService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _configuration = configuration;
    }

    /// <summary>
    /// Tiered referral bonus: 1st referral = 25 pts, 5th = 50 pts, 10th = 100 pts, otherwise 25 pts base.
    /// Called at first-payment time (not registration) — see PaymentsController.
    /// </summary>
    public static int CalculateTieredReferralBonus(int completedReferralCount)
    {
        return completedReferralCount switch
        {
            0 => 25,   // 1st referral
            4 => 50,   // 5th referral milestone
            9 => 100,  // 10th referral milestone
            _ => 25    // standard reward
        };
    }

    /// <summary>
    /// Validates the stateless HMAC-signed signup verification token.
    /// Token format: {base64(email)}:{expiry_unix}:{hmac_hex}
    /// </summary>
    private bool ValidateSignupToken(string? token, string email)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        try
        {
            var parts = token.Split(':');
            if (parts.Length != 3) return false;
            var emailB64 = parts[0];
            if (!long.TryParse(parts[1], out var expiryUnix)) return false;
            var signature = parts[2];

            // Check expiry
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiryUnix) return false;

            // Check email matches
            var tokenEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(emailB64));
            if (!string.Equals(tokenEmail, email.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)) return false;

            // Verify HMAC signature
            var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY")
                ?? _configuration["Jwt:Key"]
                ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            var payload = $"{emailB64}:{expiryUnix}";
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(jwtKey));
            var expectedSig = BitConverter.ToString(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload))).Replace("-", "").ToLower();
            return string.Equals(signature, expectedSig, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if username already exists
        if (await _userRepository.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            return Result<RegisterResponse>.FailureResult("VALIDATION_ERROR", "Username already exists");
        }

        // Check if email already exists
        if (await _userRepository.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return Result<RegisterResponse>.FailureResult("VALIDATION_ERROR", "Email already exists");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validate signup verification token (stateless HMAC — survives server restarts)
            if (!ValidateSignupToken(request.SignupVerificationToken, request.Email))
            {
                return Result<RegisterResponse>.FailureResult(
                    "EMAIL_NOT_VERIFIED",
                    "Email verification expired or invalid. Please go back and verify your email again.");
            }

            // Clean up cache entry if it still exists
            var verificationKey = $"signup-email:{request.Email.Trim().ToLowerInvariant()}";
            await _cacheService.RemoveAsync(verificationKey);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                WhatsAppNumber = request.WhatsAppNumber ?? request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _encryptionService.Hash(request.Password),
                Role = request.Role,
                IsEmailVerified = true,
                IsPhoneVerified = false,
                IsWhatsAppVerified = false,
                EmailVerificationToken = null,
                EmailVerificationTokenExpiresAt = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);

            // Create profile based on role
            if (request.Role == UserRole.Tutor)
            {
                var tutorProfile = new TutorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    VerificationStatus = VerificationStatus.NotStarted,
                    TutorReferralCode = GenerateTutorReferralCode(user.Username),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _tutorRepository.AddAsync(tutorProfile, cancellationToken);

                // Handle tutor referral code if provided at registration
                if (!string.IsNullOrWhiteSpace(request.ReferralCode))
                {
                    await HandleTutorReferralCodeAsync(request.ReferralCode, user.Id, cancellationToken);
                }
            }
            else if (request.Role == UserRole.Student)
            {
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

                // Handle referral code if provided
                if (!string.IsNullOrWhiteSpace(request.ReferralCode))
                {
                    await HandleReferralCodeAsync(request.ReferralCode, user.Id, cancellationToken);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Send welcome emails/WhatsApp — fire-and-forget, do not fail registration if email is unconfigured
            try
            {
                await _notificationService.SendWelcomeMessageAsync(user, cancellationToken);
            }
            catch { /* SMTP not configured in dev — ignore */ }

            return Result<RegisterResponse>.SuccessResult(new RegisterResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmailVerificationSent = false,
                WhatsAppVerificationSent = false
            });
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private string GenerateReferralCode(string username)
    {
        var prefix = username.ToUpper().Substring(0, Math.Min(3, username.Length));
        var unique = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"{prefix}{unique}";
    }

    private string GenerateTutorReferralCode(string username)
    {
        var prefix = username.ToUpper().Substring(0, Math.Min(3, username.Length));
        var unique = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"T{prefix}{unique}"; // T-prefix distinguishes tutor referral codes
    }

    /// <summary>
    /// Feature 16+17+20: Student referral handling.
    /// Referrer bonus is DEFERRED — awarded only after referred student makes first payment
    /// (see PaymentsController.VerifySessionPayment).
    /// Referred student still gets 25 joining bonus pts immediately.
    /// Bonus expires if first payment not made within 30 days (Feature 17).
    /// </summary>
    private async Task HandleReferralCodeAsync(string referralCode, Guid newUserId, CancellationToken cancellationToken)
    {
        try
        {
            var normalizedCode = referralCode.Trim().ToUpper();
            var referrerProfile = await _studentProfileRepository.FirstOrDefaultAsync(
                sp => sp.ReferralCode == normalizedCode, cancellationToken);

            if (referrerProfile == null) return;

            var referrerId = referrerProfile.UserId;
            if (referrerId == newUserId) return;

            const decimal joiningBonus = 25m;
            // Referrer reward is calculated at first-payment time (tiered: 25/50/100 pts)
            const decimal referralBonusPlaceholder = 25m;

            var referralProgram = new ReferralProgram
            {
                Id = Guid.NewGuid(),
                ReferrerId = referrerId,
                ReferredUserId = newUserId,
                ReferralCode = normalizedCode,
                Status = "Pending",          // Stays Pending until first payment
                RewardCredits = referralBonusPlaceholder,
                JoiningBonusAmount = joiningBonus,
                ReferralBonusPaidAt = null,  // Set at first-payment time
                RewardedAt = null,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // Feature 17: 30-day window
                IsTutorReferral = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _referralRepository.AddAsync(referralProgram, cancellationToken);

            // Credit 25 joining bonus to the newly referred student immediately
            var joiningBonusPoint = new BonusPoint
            {
                Id = Guid.NewGuid(),
                UserId = newUserId,
                Points = (int)joiningBonus,
                Reason = BonusPointReason.Referral,
                ReferenceId = referrerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _bonusPointRepository.AddAsync(joiningBonusPoint, cancellationToken);

            // Notify referrer that someone registered (but bonus is pending their first payment)
            try
            {
                await _notificationService.SendNotificationAsync(
                    referrerId,
                    "Referral Pending",
                    "Someone just registered using your referral code. You will earn bonus points when they make their first payment.",
                    NotificationType.ReferralBonus,
                    null,
                    cancellationToken);
            }
            catch { /* notification failure should not block registration */ }
        }
        catch
        {
            // Silently ignore referral errors - don't fail registration
        }
    }

    /// <summary>
    /// Feature 18: Tutor-to-tutor referral.
    /// When a new tutor registers with another tutor's TutorReferralCode, create a pending
    /// ReferralProgram (IsTutorReferral=true). Reward is credited when the referred tutor
    /// completes their first 5 sessions (handled in SessionCommandHandlers).
    /// </summary>
    private async Task HandleTutorReferralCodeAsync(string referralCode, Guid newTutorUserId, CancellationToken cancellationToken)
    {
        try
        {
            var normalizedCode = referralCode.Trim().ToUpper();
            var referrerTutor = await _tutorRepository.FirstOrDefaultAsync(
                t => t.TutorReferralCode == normalizedCode, cancellationToken);

            if (referrerTutor == null) return;
            if (referrerTutor.UserId == newTutorUserId) return;

            var referralProgram = new ReferralProgram
            {
                Id = Guid.NewGuid(),
                ReferrerId = referrerTutor.UserId,
                ReferredUserId = newTutorUserId,
                ReferralCode = normalizedCode,
                Status = "Pending",
                RewardCredits = 100m, // 100 pts awarded after referred tutor completes 5 sessions
                JoiningBonusAmount = 0m,
                ReferralBonusPaidAt = null,
                RewardedAt = null,
                ExpiresAt = DateTime.UtcNow.AddDays(90), // 90 days for tutor referrals
                IsTutorReferral = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _referralRepository.AddAsync(referralProgram, cancellationToken);
        }
        catch
        {
            // Silently ignore - don't fail registration
        }
    }
}

