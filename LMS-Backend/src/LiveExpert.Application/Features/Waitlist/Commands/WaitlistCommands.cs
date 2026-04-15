using LiveExpert.Application.Common;
using LiveExpert.Domain.Enums;
using MediatR;

namespace LiveExpert.Application.Features.Waitlist.Commands;

// Feature 8: Waitlist for full group sessions

public class JoinWaitlistCommand : IRequest<Result<WaitlistDto>>
{
    public Guid SessionId { get; set; }
}

public class GetWaitlistPositionQuery : IRequest<Result<WaitlistDto>>
{
    public Guid SessionId { get; set; }
}

public class WaitlistDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }
    public int Position { get; set; }
    public WaitlistStatus Status { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
