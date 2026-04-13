using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Auth.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;

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
        ICacheService cacheService)
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
            // Create user
            var verificationKey = $"signup-email:{request.Email.Trim().ToLowerInvariant()}";
            var verificationState = await _cacheService.GetAsync<SignupEmailVerificationState>(verificationKey);
            if (verificationState == null ||
                !verificationState.Verified ||
                verificationState.VerificationToken != request.SignupVerificationToken ||
                verificationState.ExpiresAt < DateTime.UtcNow)
            {
                return Result<RegisterResponse>.FailureResult(
                    "EMAIL_NOT_VERIFIED",
                    "Please verify your email before completing signup.");
            }

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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _tutorRepository.AddAsync(tutorProfile, cancellationToken);
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

            await _cacheService.RemoveAsync(verificationKey);

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

    private async Task HandleReferralCodeAsync(string referralCode, Guid newUserId, CancellationToken cancellationToken)
    {
        try
        {
            var normalizedCode = referralCode.Trim().ToUpper();
            // Find referrer by referral code
            var referrerProfile = await _studentProfileRepository.FirstOrDefaultAsync(
                sp => sp.ReferralCode == normalizedCode, cancellationToken);

            if (referrerProfile == null)
            {
                // Invalid referral code - silently ignore (don't fail registration)
                return;
            }

            var referrerId = referrerProfile.UserId;

            // Don't allow self-referral
            if (referrerId == newUserId)
            {
                return;
            }

            // Referral bonus released after referred student books a session
            var referralBonus = await _settingsService.GetReferralBonusCreditsAsync();

            // Create referral program record (status: Pending - will be rewarded on first booking)
            var referralProgram = new ReferralProgram
            {
                Id = Guid.NewGuid(),
                ReferrerId = referrerId,
                ReferredUserId = newUserId,
                ReferralCode = normalizedCode,
                Status = "Pending", // Will be changed to "Completed" when referred user makes first purchase
                RewardCredits = referralBonus,
                JoiningBonusAmount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _referralRepository.AddAsync(referralProgram, cancellationToken);
        }
        catch
        {
            // Silently ignore referral errors - don't fail registration
        }
    }
}
