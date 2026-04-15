using LiveExpert.Application.Common;
using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LiveExpert.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<SessionBooking> _bookingRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<TutorEarning> _tutorEarningRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public PaymentsController(
        IRepository<Payment> paymentRepository,
        IRepository<SessionBooking> bookingRepository,
        IRepository<Session> sessionRepository,
        IRepository<User> userRepository,
        IRepository<TutorEarning> tutorEarningRepository,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        INotificationDispatcher notificationDispatcher,
        IUnitOfWork unitOfWork,
        IConfiguration config)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _tutorEarningRepository = tutorEarningRepository;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _notificationDispatcher = notificationDispatcher;
        _unitOfWork = unitOfWork;
        _config = config;
    }

    /// <summary>
    /// Verify Razorpay payment for a session booking
    /// </summary>
    [HttpPost("sessions/verify")]
    [Authorize]
    public async Task<IActionResult> VerifySessionPayment([FromBody] VerifySessionPaymentRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result.FailureResult("UNAUTHORIZED", "User not authenticated"));
        }

        var payment = await _paymentRepository.FirstOrDefaultAsync(
            p => p.GatewayOrderId == request.RazorpayOrderId,
            cancellationToken);

        if (payment == null || payment.StudentId != userId.Value)
        {
            return NotFound(Result.FailureResult("NOT_FOUND", "Payment not found"));
        }

        var isValid = await _paymentService.VerifyPaymentSignatureAsync(
            request.RazorpayOrderId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (!isValid)
        {
            return BadRequest(Result.FailureResult("INVALID_SIGNATURE", "Payment verification failed"));
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            payment.Status = PaymentStatus.Success;
            payment.GatewayPaymentId = request.RazorpayPaymentId;
            payment.GatewaySignature = request.RazorpaySignature;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            var booking = await _bookingRepository.FirstOrDefaultAsync(
                b => b.PaymentId == payment.Id,
                cancellationToken);

            if (booking != null)
            {
                booking.BookingStatus = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(booking, cancellationToken);
            }

            var commissionPercentage = payment.BaseAmount > 0
                ? Math.Round((payment.PlatformFee / payment.BaseAmount) * 100m, 2)
                : 0m;

            var releaseDelayDays = _config.GetValue<int>("AppSettings:SessionCreditReleaseDelayDays", 3);

            var earning = new TutorEarning
            {
                Id = Guid.NewGuid(),
                TutorId = payment.TutorId,
                SourceType = "Session",
                SourceId = payment.SessionId,
                BookingId = booking?.Id,
                Amount = payment.BaseAmount,
                CommissionPercentage = commissionPercentage,
                CommissionAmount = payment.PlatformFee,
                Status = EarningStatus.Pending,
                AvailableAt = DateTime.UtcNow.AddDays(releaseDelayDays),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tutorEarningRepository.AddAsync(earning, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Send dynamic email notification for payment success
            try
            {
                var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
                var session = await _sessionRepository.GetByIdAsync(payment.SessionId, cancellationToken);
                
                if (user != null && session != null)
                {
                    var userName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(userName)) userName = user.Username;

                    var emailBody = EmailTemplates.PaymentSuccessEmail(
                        userName, 
                        session.Title, 
                        payment.TotalAmount, 
                        payment.GatewayPaymentId ?? request.RazorpayPaymentId);

                    await _notificationDispatcher.SendAsync(new NotificationDispatchRequest
                    {
                        UserId = user.Id,
                        Category = NotificationCategory.Payment,
                        IsTransactional = true,
                        Title = "Payment Successful",
                        Message = $"Your payment for \"{session.Title}\" was successful.",
                        EmailTo = user.Email,
                        EmailSubject = $"Payment Successful: {session.Title}",
                        EmailBody = emailBody,
                        EmailIsHtml = true,
                        WhatsAppTo = user.WhatsAppNumber ?? user.PhoneNumber,
                        WhatsAppMessage = $"💳 Payment Confirmed!\n\nHi {userName}, your payment of ₹{payment.TotalAmount:N2} for *{session.Title}* was successful.\n\nBooking is confirmed. Join on {session.ScheduledAt:dd MMM yyyy} at {session.ScheduledAt:HH:mm}.\n\nSee you there! 🎓",
                        SendInApp = true
                    }, cancellationToken);
                    // Notify Tutor as well
                    var tutor = await _userRepository.GetByIdAsync(payment.TutorId, cancellationToken);
                    if (tutor != null)
                    {
                        var tutorName = $"{tutor.FirstName} {tutor.LastName}".Trim();
                        if (string.IsNullOrEmpty(tutorName)) tutorName = tutor.Username;

                        await _notificationDispatcher.SendAsync(new NotificationDispatchRequest
                        {
                            UserId = tutor.Id,
                            Category = NotificationCategory.SessionBooking,
                            IsTransactional = true,
                            Title = "Session Booked & Paid",
                            Message = $"{userName} has booked and paid for \"{session.Title}\".",
                            ActionUrl = "/tutor/sessions",
                            EmailTo = tutor.Email,
                            EmailSubject = "New Confirmed Booking",
                            EmailBody = $"<p>Hello,</p><p>{userName} has booked and paid for your session \"{session.Title}\".</p><p>Scheduled for: {session.ScheduledAt:f}</p><p><a href='https://liveexpert.ai/tutor/sessions'>View your sessions</a></p>",
                            EmailIsHtml = true,
                            WhatsAppTo = tutor.WhatsAppNumber ?? tutor.PhoneNumber,
                            WhatsAppMessage = $"🎉 New Booking!\n\nHi {tutorName}, *{userName}* has booked and paid for your session *{session.Title}*.\n\nScheduled: {session.ScheduledAt:dd MMM yyyy} at {session.ScheduledAt:HH:mm}\n\nView at LiveExpert.ai/tutor/sessions",
                            SendInApp = true
                        }, cancellationToken);
                    }
                }
            }
            catch (Exception)
            {
                // Don't fail the verification if notification fails
            }

            return Ok(Result.SuccessResult("Payment verified"));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public class VerifySessionPaymentRequest
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}
