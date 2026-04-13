using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Sessions.Commands;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Sessions.Queries;

// Get Sessions Query
public class GetSessionsQuery : IRequest<Result<PaginatedResult<SessionDto>>>
{
    public SessionStatus? Status { get; set; }
    public Guid? TutorId { get; set; }
    public Guid? StudentId { get; set; }
    public bool? Upcoming { get; set; }
    public bool? Past { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Get Session By Id Query
public class GetSessionByIdQuery : IRequest<Result<SessionDetailDto>>
{
    public Guid SessionId { get; set; }
}

// Get Session Meeting Link Query
public class GetSessionMeetingLinkQuery : IRequest<Result<MeetingLinkDto>>
{
    public Guid SessionId { get; set; }
}

// Get Session Pricing Query
public class GetSessionPricingQuery : IRequest<Result<SessionPricingDto>>
{
    public Guid SessionId { get; set; }
    public int? Hours { get; set; }
}

// DTOs
public class SessionDetailDto : SessionDto
{
    public List<BookingDetailDto> Bookings { get; set; } = new();
    public string? RecordingUrl { get; set; }
    public bool IsRecorded { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class BookingDetailDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentImage { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public bool AttendanceMarked { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

public class MeetingLinkDto
{
    public string MeetingLink { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int Duration { get; set; }
}

public class SessionPricingDto
{
    public int? Hours { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal TotalAmount { get; set; }
}
