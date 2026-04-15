using LiveExpert.Application.Common;
using LiveExpert.Application.Features.Sessions.Commands;
using LiveExpert.Application.Features.Sessions.Queries;
using LiveExpert.API.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetSessionStatusQuery = LiveExpert.Application.Features.Sessions.Queries.GetSessionStatusQuery;

namespace LiveExpert.API.Controllers;

[Route("api/[controller]")]
public class SessionsController : BaseController
{
    public SessionsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Create a new session (Tutor only) - Requires Google Calendar consent
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(SessionDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? CreatedAtAction(nameof(GetSession), new { sessionId = result.Data!.Id }, result) : HandleResult(result);
    }

    /// <summary>
    /// Get sessions list with filters (Public endpoint for browsing)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResult<SessionDto>), 200)]
    public async Task<IActionResult> GetSessions([FromQuery] GetSessionsQuery query)
    {
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get session details by ID
    /// </summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(SessionDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        var result = await _mediator.Send(new GetSessionByIdQuery { SessionId = sessionId });
        return HandleResult(result);
    }

    /// <summary>
    /// Update session (Tutor only)
    /// </summary>
    [HttpPut("{sessionId}")]
    [ProducesResponseType(typeof(SessionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSession(Guid sessionId, [FromBody] UpdateSessionCommand command)
    {
        command.SessionId = sessionId;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel session (Tutor only)
    /// </summary>
    [HttpDelete("{sessionId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelSession(Guid sessionId, [FromBody] CancelSessionCommand command)
    {
        command.SessionId = sessionId;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Book a session (Student only) - Requires Google Calendar consent
    /// </summary>
    [HttpPost("{sessionId}/book")]
    [RequireCalendarConsent]
    [ProducesResponseType(typeof(BookingDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> BookSession(Guid sessionId, [FromBody] BookSessionCommand command)
    {
        command.SessionId = sessionId;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get pricing breakdown for a session
    /// </summary>
    [HttpPost("{sessionId}/pricing")]
    [ProducesResponseType(typeof(SessionPricingDto), 200)]
    public async Task<IActionResult> GetSessionPricing(Guid sessionId, [FromBody] GetSessionPricingQuery query)
    {
        query.SessionId = sessionId;
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel session booking (Student only)
    /// </summary>
    [HttpPost("{sessionId}/cancel-booking")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelBooking(Guid sessionId, [FromBody] CancelBookingCommand command)
    {
        command.SessionId = sessionId;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Respond to session booking request (Tutor only)
    /// </summary>
    [HttpPost("{sessionId}/bookings/{bookingId}/respond")]
    [Authorize(Roles = "Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RespondBooking(Guid sessionId, Guid bookingId, [FromBody] RespondBookingCommand command)
    {
        command.SessionId = sessionId;
        command.BookingId = bookingId;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Mark student attendance (Tutor only)
    /// </summary>
    [HttpPost("{sessionId}/mark-attendance")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAttendance(Guid sessionId, [FromBody] MarkAttendanceCommand command)
    {
        command.SessionId = sessionId;
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Get meeting link for session
    /// </summary>
    [HttpGet("{sessionId}/meeting-link")]
    [ProducesResponseType(typeof(MeetingLinkDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMeetingLink(Guid sessionId)
    {
        var result = await _mediator.Send(new GetSessionMeetingLinkQuery { SessionId = sessionId });
        return HandleResult(result);
    }

    /// <summary>
    /// Start session - Tutor only (returns temporary Meet URL) - Requires Google Calendar consent
    /// </summary>
    [HttpPost("{sessionId}/start")]
    [Authorize(Roles = "Tutor")]
    [RequireCalendarConsent]
    [ProducesResponseType(typeof(Result<LiveExpert.Application.Features.Sessions.Commands.StartSessionResponse>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> StartSession(Guid sessionId)
    {
        var result = await _mediator.Send(new StartSessionCommand { SessionId = sessionId });
        return HandleResult(result);
    }

    /// <summary>
    /// Join session - Student only (returns temporary Meet URL)
    /// Students use the tutor's meet link — no calendar consent required on their side
    /// </summary>
    [HttpPost("{sessionId}/join")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(Result<LiveExpert.Application.Features.Sessions.Commands.JoinSessionResponse>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> JoinSession(Guid sessionId)
    {
        var result = await _mediator.Send(new JoinSessionCommand { SessionId = sessionId });
        return HandleResult(result);
    }

    /// <summary>
    /// Get session status (for waiting room UI)
    /// </summary>
    [HttpGet("{sessionId}/status")]
    [ProducesResponseType(typeof(Result<LiveExpert.Application.Features.Sessions.Queries.SessionStatusResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSessionStatus(Guid sessionId)
    {
        var result = await _mediator.Send(new GetSessionStatusQuery { SessionId = sessionId });
        return HandleResult(result);
    }

    /// <summary>
    /// Mark session as completed (Tutor only)
    /// </summary>
    [HttpPost("{sessionId}/complete")]
    [Authorize(Roles = "Tutor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkComplete(Guid sessionId)
    {
        var result = await _mediator.Send(new CompleteSessionCommand { SessionId = sessionId });
        return HandleResult(result);
    }
}

// Note: DTOs are defined in the Command/Query handlers
