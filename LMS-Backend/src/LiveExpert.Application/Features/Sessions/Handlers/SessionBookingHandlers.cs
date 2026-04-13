using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Sessions.Commands;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LiveExpert.Application.Features.Sessions.Handlers;

public class BookSessionCommandHandler : IRequestHandler<BookSessionCommand, Result<BookingDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<ReferralProgram> _referralRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<BonusPoint> _bonusPointRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationHelper;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly ISystemSettingsService _settingsService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookSessionCommandHandler> _logger;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly IApiSettingService _apiSettingService;

    public BookSessionCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<ReferralProgram> referralRepository,
        IRepository<Payment> paymentRepository,
        IRepository<BonusPoint> bonusPointRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        INotificationDispatcher notificationDispatcher,
        ISystemSettingsService settingsService,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<BookSessionCommandHandler> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        IApiSettingService apiSettingService)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _referralRepository = referralRepository;
        _paymentRepository = paymentRepository;
        _bonusPointRepository = bonusPointRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationHelper = notificationService;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _notificationDispatcher = notificationDispatcher;
        _settingsService = settingsService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
        _apiSettingService = apiSettingService;
    }

    public async Task<Result<BookingDto>> Handle(BookSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<BookingDto>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
            {
                return Result<BookingDto>.FailureResult("NOT_FOUND", "Session not found");
            }

            // Check if session is full
            if (session.CurrentStudents >= session.MaxStudents)
            {
                return Result<BookingDto>.FailureResult("SESSION_FULL", "Session is already full");
            }

            // Check if already booked
            var existingBooking = await _bookingRepository.FirstOrDefaultAsync(
                b => b.SessionId == request.SessionId && b.StudentId == userId.Value && b.BookingStatus != BookingStatus.Cancelled,
                cancellationToken);

            if (existingBooking != null)
            {
                // If the booking is already confirmed (Paid), then it's a real duplicate
                if (existingBooking.BookingStatus == BookingStatus.Confirmed)
                {
                    return Result<BookingDto>.FailureResult("ALREADY_BOOKED", "You have already booked and confirmed this session");
                }

                // If it's pending, we allow them to RETRY the payment
                // This handles cases where they closed the window or the network glitched
                var paymentId = existingBooking.PaymentId;
                if (paymentId != null)
                {
                    var existingPayment = await _paymentRepository.GetByIdAsync(paymentId.Value, cancellationToken);
                    if (existingPayment != null && existingPayment.Status != PaymentStatus.Success)
                    {
                        var keyId = await _apiSettingService.GetApiSettingAsync("Razorpay", "KeyId") 
                            ?? await _apiSettingService.GetApiSettingAsync("Razorpay", "Id")
                            ?? _configuration["Razorpay:KeyId"];

                         _logger.LogInformation("Returning existing pending booking for payment retry: {BookingId}", existingBooking.Id);

                        return Result<BookingDto>.SuccessResult(new BookingDto
                        {
                            Id = existingBooking.Id,
                            SessionId = existingBooking.SessionId,
                            Status = existingBooking.BookingStatus,
                            HoursBooked = existingBooking.HoursBooked,
                            BaseAmount = existingBooking.BaseAmount,
                            PlatformFee = existingBooking.PlatformFee,
                            TotalAmount = existingBooking.TotalAmount,
                            RazorpayOrderId = existingPayment.GatewayOrderId,
                            RazorpayKey = keyId,
                            CreatedAt = existingBooking.CreatedAt
                        });
                    }
                }

                return Result<BookingDto>.FailureResult("ALREADY_BOOKED", "You have already booked this session. Please check your sessions dashboard.");
            }

            int? hoursBooked = null;
            if (session.PricingType == SessionPricingType.Hourly)
            {
                if (!request.Hours.HasValue || request.Hours.Value <= 0)
                {
                    return Result<BookingDto>.FailureResult("INVALID_HOURS", "Hours are required for hourly sessions");
                }
                hoursBooked = request.Hours.Value;
            }

            var baseAmount = session.PricingType == SessionPricingType.Hourly
                ? session.BasePrice * (hoursBooked ?? 0)
                : session.BasePrice;

            var platformFee = 0m;
            var bookingCount = await _bookingRepository.CountAsync(b => b.StudentId == userId.Value, cancellationToken);
            var isFirstBooking = bookingCount == 0;
            
            var platformFeeEnabled = await _settingsService.IsPlatformFeeEnabledAsync();
            if (platformFeeEnabled && isFirstBooking)
            {
                var feeType = await _settingsService.GetPlatformFeeTypeAsync();
                switch (feeType)
                {
                    case PlatformFeeType.Fixed:
                        platformFee = await _settingsService.GetPlatformFeeFixedAsync();
                        break;
                    case PlatformFeeType.PerHour:
                        platformFee = await _settingsService.GetPlatformFeePerHourAsync() * (hoursBooked ?? 1);
                        break;
                    case PlatformFeeType.Percentage:
                        var percentage = await _settingsService.GetPlatformFeePercentageAsync();
                        platformFee = Math.Round(baseAmount * (percentage / 100m), 2);
                        break;
                }
            }

            var totalAmount = baseAmount + platformFee;

            // Create booking as pending until tutor approves
            var booking = new SessionBooking
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                StudentId = userId.Value,
                BookingStatus = BookingStatus.Pending,
                HoursBooked = hoursBooked,
                BaseAmount = baseAmount,
                PlatformFee = platformFee,
                TotalAmount = totalAmount,
                AttendanceMarked = false,
                SpecialInstructions = request.SpecialInstructions,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking, cancellationToken);

            var metadata = new Dictionary<string, string>
            {
                { "sessionId", session.Id.ToString() },
                { "bookingId", booking.Id.ToString() },
                { "studentId", userId.Value.ToString() },
                { "tutorId", session.TutorId.ToString() }
            };

            var (orderId, key) = await _paymentService.CreateOrderAsync(totalAmount, "INR", metadata);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                StudentId = userId.Value,
                TutorId = session.TutorId,
                SessionId = session.Id,
                BaseAmount = baseAmount,
                PlatformFee = platformFee,
                TotalAmount = totalAmount,
                Status = PaymentStatus.Pending,
                PaymentGateway = "Razorpay",
                GatewayOrderId = orderId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
            booking.PaymentId = payment.Id;
            
            // Update session students count
            session.CurrentStudents++;

            var tutorUser = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
            var studentUser = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

            await TryReleaseReferralBonusesAsync(userId.Value, session.Id, cancellationToken);

            // Attempt to commit transaction with a single retry for concurrency issues
            try
            {
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error during booking commit. Retrying once...");
                // Reload entities and try one last time or just return success if already saved
                // For now, let's try to just commit again after a tiny delay
                await Task.Delay(100);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            // Send notifications after transaction is successfully committed
            try
            {
                if (tutorUser != null && studentUser != null)
                {
                    var studentName = $"{studentUser.FirstName} {studentUser.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(studentName))
                    {
                        studentName = studentUser.Username;
                    }

                    var tutorName = $"{tutorUser.FirstName} {tutorUser.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(tutorName))
                    {
                        tutorName = tutorUser.Username;
                    }

                    var sessionTime = session.ScheduledAt.ToString("f");
                    var requestLink = $"https://liveexpert.ai/tutor/sessions/{session.Id}";
                    var (subject, body) = NotificationTemplates.StudentRequestedSessionEmail(
                        tutorName,
                        studentName,
                        session.Title,
                        sessionTime,
                        requestLink);

                    /* Tutor notification moved to PaymentsController.VerifySessionPayment to only notify when confirmed */

                    // Send email to student
                    var studentEmailBody = EmailTemplates.ThanksForBookingEmail(studentName);

                    await _notificationDispatcher.SendAsync(new NotificationDispatchRequest
                    {
                        UserId = studentUser.Id,
                        Category = NotificationCategory.SessionBooking,
                        IsTransactional = true,
                        Title = "Booking Initialized",
                        Message = "Please complete your payment to confirm your booking.",
                        ActionUrl = "/student/my-sessions",
                        EmailTo = studentUser.Email,
                        EmailSubject = "Thanks for Booking Your Session",
                        EmailBody = studentEmailBody,
                        EmailIsHtml = true,
                        WhatsAppTo = studentUser.WhatsAppNumber ?? studentUser.PhoneNumber,
                        WhatsAppMessage = $"📚 Booking Initiated!\n\nHi {studentUser.FirstName}, your booking for *{session.Title}* has been initialized.\n\nPlease complete your payment to confirm the session. Visit LiveExpert.ai to proceed.",
                        SendInApp = true
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // We don't want to fail the whole booking if only notifications failed
                // Since the transaction is already committed, the booking is successful.
                // Log the exception but continue.
            }

            return Result<BookingDto>.SuccessResult(new BookingDto
            {
                Id = booking.Id,
                SessionId = booking.SessionId,
                Status = booking.BookingStatus,
                HoursBooked = booking.HoursBooked,
                BaseAmount = booking.BaseAmount,
                PlatformFee = booking.PlatformFee,
                TotalAmount = booking.TotalAmount,
                RazorpayOrderId = orderId,
                RazorpayKey = key,
                CreatedAt = booking.CreatedAt
            });
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task TryReleaseReferralBonusesAsync(Guid studentId, Guid sessionId, CancellationToken cancellationToken)
    {
        var referral = await _referralRepository.FirstOrDefaultAsync(
            r => r.ReferredUserId == studentId && r.Status == "Pending",
            cancellationToken);

        if (referral == null)
        {
            return;
        }

        if (referral.ReferralBonusPaidAt == null)
        {
            var bonusPoint = new BonusPoint
            {
                Id = Guid.NewGuid(),
                UserId = referral.ReferrerId,
                Points = (int)Math.Round(referral.RewardCredits),
                Reason = BonusPointReason.Referral,
                ReferenceId = sessionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _bonusPointRepository.AddAsync(bonusPoint, cancellationToken);
            referral.ReferralBonusPaidAt = DateTime.UtcNow;

            await _notificationHelper.SendNotificationAsync(
                referral.ReferrerId,
                "Referral Bonus Earned",
                $"You earned {referral.RewardCredits:N0} points for a completed referral booking.",
                NotificationType.ReferralBonus,
                null,
                cancellationToken);
        }
        if (referral.ReferralBonusPaidAt != null)
        {
            referral.Status = "Completed";
            referral.RewardedAt = DateTime.UtcNow;
        }

        referral.UpdatedAt = DateTime.UtcNow;
        await _referralRepository.UpdateAsync(referral, cancellationToken);
    }
}

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationHelper;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationHelper = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var booking = await _bookingRepository.FirstOrDefaultAsync(
                b => b.SessionId == request.SessionId && b.StudentId == userId.Value,
                cancellationToken);

            if (booking == null)
            {
                return Result.FailureResult("NOT_FOUND", "Booking not found");
            }

            if (booking.BookingStatus == BookingStatus.Cancelled)
            {
                return Result.FailureResult("ALREADY_CANCELLED", "Booking already cancelled");
            }

            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
            {
                return Result.FailureResult("NOT_FOUND", "Session not found");
            }

            // Update booking
            booking.BookingStatus = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking, cancellationToken);

            // Update session
            session.CurrentStudents--;
            await _sessionRepository.UpdateAsync(session, cancellationToken);

            // Notify tutor
            var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
            if (tutor != null)
            {
                var studentName = _currentUserService.Username ?? "A student";
                await _notificationHelper.SendSessionCancelledAsync(
                    tutor,
                    session.Title,
                    session.ScheduledAt,
                    studentName,
                    cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.SuccessResult("Booking cancelled successfully");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public class RespondBookingCommandHandler : IRequestHandler<RespondBookingCommand, Result<BookingDto>>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationHelper;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUnitOfWork _unitOfWork;

    public RespondBookingCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        INotificationDispatcher notificationDispatcher,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationHelper = notificationService;
        _notificationDispatcher = notificationDispatcher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BookingDto>> Handle(RespondBookingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<BookingDto>.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        if (request.Status != BookingStatus.Confirmed && request.Status != BookingStatus.Cancelled)
        {
            return Result<BookingDto>.FailureResult("INVALID_STATUS", "Only accept or reject is allowed");
        }

        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Result<BookingDto>.FailureResult("NOT_FOUND", "Session not found");
        }

        if (session.TutorId != userId.Value)
        {
            return Result<BookingDto>.FailureResult("FORBIDDEN", "Only the tutor can respond to requests");
        }

        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null || booking.SessionId != session.Id)
        {
            return Result<BookingDto>.FailureResult("NOT_FOUND", "Booking not found");
        }

        if (booking.BookingStatus != BookingStatus.Pending)
        {
            return Result<BookingDto>.FailureResult("INVALID_STATUS", "Only pending bookings can be updated");
        }

        booking.BookingStatus = request.Status;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        if (request.Status == BookingStatus.Cancelled && session.CurrentStudents > 0)
        {
            session.CurrentStudents--;
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var statusLabel = request.Status == BookingStatus.Confirmed ? "accepted" : "rejected";
        await _notificationHelper.SendNotificationAsync(
            booking.StudentId,
            "Session Request Update",
            $"Your session request for \"{session.Title}\" was {statusLabel}.",
            NotificationType.SessionBooked,
            $"/student/sessions/{session.Id}",
            cancellationToken);

        if (request.Status == BookingStatus.Confirmed)
        {
            var student = await _userRepository.GetByIdAsync(booking.StudentId, cancellationToken);
            var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
            if (student != null && tutor != null)
            {
                var studentName = $"{student.FirstName} {student.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(studentName))
                {
                    studentName = student.Username;
                }

                var tutorName = $"{tutor.FirstName} {tutor.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(tutorName))
                {
                    tutorName = tutor.Username;
                }

                var sessionLink = $"https://liveexpert.ai/sessions/{session.Id}";
                
                // Notify Student
                await _notificationHelper.SendSessionScheduledAsync(student, session, sessionLink, tutorName, cancellationToken);
                
                // Notify Tutor
                await _notificationHelper.SendSessionScheduledAsync(tutor, session, sessionLink, studentName, cancellationToken);
            }
        }

        return Result<BookingDto>.SuccessResult(new BookingDto
        {
            Id = booking.Id,
            SessionId = booking.SessionId,
            Status = booking.BookingStatus,
            HoursBooked = booking.HoursBooked,
            BaseAmount = booking.BaseAmount,
            PlatformFee = booking.PlatformFee,
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt
        });
    }
}

public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Result>
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly INotificationService _notificationHelper;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAttendanceCommandHandler(
        IRepository<Session> sessionRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _notificationHelper = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result.FailureResult("UNAUTHORIZED", "User not authenticated");
        }

        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Result.FailureResult("NOT_FOUND", "Session not found");
        }

        // Allow both tutor and student to mark attendance
        var isTutor = session.TutorId == userId.Value;
        var isStudent = request.StudentId == userId.Value;
        
        if (!isTutor && !isStudent)
        {
            return Result.FailureResult("FORBIDDEN", "You don't have permission to mark attendance for this session");
        }

        var booking = await _bookingRepository.FirstOrDefaultAsync(
            b => b.SessionId == request.SessionId && b.StudentId == request.StudentId,
            cancellationToken);

        if (booking == null)
        {
            return Result.FailureResult("NOT_FOUND", "Booking not found");
        }

        booking.AttendanceMarked = true;
        booking.Attended = request.Attended;
        booking.JoinedAt = request.JoinedAt;
        booking.LeftAt = request.LeftAt;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send feedback email if attended
        if (booking.Attended)
        {
            var student = await _userRepository.GetByIdAsync(booking.StudentId, cancellationToken);
            var tutor = await _userRepository.GetByIdAsync(session.TutorId, cancellationToken);
            if (student != null && tutor != null)
            {
                var feedbackLink = $"https://liveexpert.ai/sessions/{session.Id}/feedback";
                var tutorName = $"{tutor.FirstName} {tutor.LastName}";
                await _notificationHelper.SendSessionFeedbackAsync(student, tutorName, session.Title, feedbackLink, cancellationToken);
            }
        }

        return Result.SuccessResult("Attendance marked successfully");
    }
}
