using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Sessions.Commands;

// Create Session Command
public class CreateSessionCommand : IRequest<Result<SessionDto>>
{
    public SessionType SessionType { get; set; }
    public SessionPricingType PricingType { get; set; } = SessionPricingType.Fixed;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SubjectId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int Duration { get; set; }
    public decimal BasePrice { get; set; }
    public int MaxStudents { get; set; } = 1;
}

// Update Session Command
public class UpdateSessionCommand : IRequest<Result<SessionDto>>
{
    public Guid SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int Duration { get; set; }
    public decimal BasePrice { get; set; }
    public SessionPricingType PricingType { get; set; } = SessionPricingType.Fixed;
}

// Cancel Session Command
public class CancelSessionCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
    public string? Reason { get; set; }
}

// Book Session Command
public class BookSessionCommand : IRequest<Result<BookingDto>>
{
    public Guid SessionId { get; set; }
    public string? SpecialInstructions { get; set; }
    public int? Hours { get; set; }
    /// <summary>When true, available bonus points are applied as a discount (1 point = ₹1).</summary>
    public bool UsePoints { get; set; } = false;
}

// Cancel Booking Command
public class CancelBookingCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
    public string? Reason { get; set; }
}

// Respond to Booking Command (Tutor only)
public class RespondBookingCommand : IRequest<Result<BookingDto>>
{
    public Guid SessionId { get; set; }
    public Guid BookingId { get; set; }
    public BookingStatus Status { get; set; }
}

// Mark Attendance Command
public class MarkAttendanceCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public bool Attended { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

// Complete Session Command
public class CompleteSessionCommand : IRequest<Result>
{
    public Guid SessionId { get; set; }
}

// DTOs
public class SessionDto
{
    public Guid Id { get; set; }
    public Guid TutorId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string TutorImage { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public SessionPricingType PricingType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int Duration { get; set; }
    public decimal BasePrice { get; set; }
    public int MaxStudents { get; set; }
    public int CurrentStudents { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public bool IsBooked { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public BookingStatus Status { get; set; }
    public int? HoursBooked { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal PointsDiscount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayKey { get; set; }
    public DateTime CreatedAt { get; set; }
}
